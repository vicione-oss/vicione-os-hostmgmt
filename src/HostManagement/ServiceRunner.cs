using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using HostManagement.Common.Wrappers;
using HostManagement.Communication.NamedPipes;
using HostManagement.Communication.NamedPipes.Extensions;
using HostManagement.Configuration;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig;

namespace HostManagement;

/// <summary>
/// Represents the <see cref="ServiceRunner"/>, which is responsible for starting the service or
/// preparing the configuration at boot time.
/// </summary>
internal static partial class ServiceRunner
{
    private static readonly ILogger s_logger = new ConsoleLogger(EnvironmentVariables.LogLevel);
    private static readonly IFileSystem s_fileSystem = new FileSystem();
    private static readonly FileSystemWrapper s_fileSystemWrapper = new(s_fileSystem, s_logger);
    private static readonly string s_setConfigFlag = Path.Combine(AppPaths.TempPath, "set-config.flag");

    /// <summary>
    /// Starts the "prepare configuration" mode of the application, that should run at first boot.
    /// </summary>
    /// <returns>Always returns 0 (graceful exit on error)</returns>
    internal static async Task<int> PrepareConfiguration()
    {
        try
        {
            if (!TryInitializeService())
            {
                // gracefully exit application
                return 0;
            }

            LogPrepareConfigurationAtBootTime(s_logger);

            var configurationService = new SystemConfigurationService(s_fileSystem);

            var prepareConfigResult = await configurationService.PrepareConfiguration();

            if (prepareConfigResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, prepareConfigResult.Message);

                // gracefully exit application
                return 0;
            }

            await CreateSetConfigFlag();

            if (prepareConfigResult.Status == OperationStatus.Warning)
                LogResultReturnedWarning(s_logger, prepareConfigResult.Message);

            if (prepareConfigResult.Status == OperationStatus.Success)
                LogResultReturnedSuccess(s_logger, prepareConfigResult.Message);

            return 0;
        }
        catch (Exception ex)
        {
            LogServiceError(s_logger, ex.ToString());

            // gracefully exit application
            return 0;
        }
    }

    /// <summary>
    /// Starts the application as a service, that runs in the background.
    /// </summary>
    /// <returns>0 if service ended normally, otherwise 1</returns>
    internal static async Task<int> StartService()
    {
        try
        {
            if (!TryInitializeService())
                return 1;

            using ConsoleLifetime lifetime = new(s_logger);

            if (s_fileSystemWrapper.ExistsFile(s_setConfigFlag))
            {
                DeleteSetConfigFlag();

                var getJsonConfig = await GetJsonConfigurationOnStartUp()
                    .ContinueWith(task =>
                    {
                        if (task.Result.Status == OperationStatus.Error)
                            lifetime.Cancel();

                        return task.Result.Configuration;
                    },
                    TaskScheduler.Current);

                if (getJsonConfig is SystemConfiguration jsonConfig)
                {
                    if (!await SetJsonConfigurationOnStartUp(jsonConfig, lifetime.Token))
                        return 1;
                }
            }

            await StartNamedPipeServer(lifetime.Token);

            return 0;
        }
        catch (Exception ex)
        {
            LogServiceError(s_logger, ex.ToString());
            return 1;
        }
    }

    private static async Task<GetSystemConfigurationResult> GetJsonConfigurationOnStartUp()
    {
        if (s_fileSystemWrapper.ExistsFile(AppPaths.SystemConfigurationFilePath))
            LogFoundExistingSystemConfiguration(s_logger, AppPaths.SystemConfigurationFilePath);
        else if (s_fileSystemWrapper.ExistsFile(AppPaths.DefaultSystemConfigurationFilePath))
            LogFoundExistingSystemConfiguration(s_logger, AppPaths.DefaultSystemConfigurationFilePath);
        else
            LogCreateSystemConfigurationStarted(s_logger);

        var configurationService = new SystemConfigurationService(s_fileSystem);
        var getJsonConfigResult = await configurationService.GetConfigurationFromJson();

        if (getJsonConfigResult.Status == OperationStatus.Error)
            LogResultReturnedError(s_logger, getJsonConfigResult.Message);

        if (getJsonConfigResult.Status == OperationStatus.Warning)
            LogResultReturnedWarning(s_logger, getJsonConfigResult.Message);

        if (getJsonConfigResult.Status == OperationStatus.Success)
            LogResultReturnedSuccess(s_logger, getJsonConfigResult.Message);

        return getJsonConfigResult;
    }

    private static async Task<bool> SetJsonConfigurationOnStartUp(
        SystemConfiguration jsonConfig,
        CancellationToken cancellationToken)
    {
        var configurationService = new SystemConfigurationService(s_fileSystem);
        var setConfigResult = await configurationService
            .SetConfiguration(jsonConfig, shouldPersistConfiguration: false, cancellationToken);

        if (setConfigResult.Status == OperationStatus.Error)
        {
            LogResultReturnedError(s_logger, setConfigResult.Message);
            return false;
        }

        if (setConfigResult.Status == OperationStatus.Warning)
            LogResultReturnedWarning(s_logger, setConfigResult.Message);

        if (setConfigResult.Status == OperationStatus.Success)
            LogResultReturnedSuccess(s_logger, setConfigResult.Message);

        return true;
    }

    private static async Task StartNamedPipeServer(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var pipeServer = new PipeServer("HostPipe", s_logger);

                await pipeServer.SetExecutePermission();
                await pipeServer.SetOwnership();
                await pipeServer.WaitForConnection(cancellationToken).ConfigureAwait(false);
                await pipeServer.WaitForRemotePipeClose(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        { }
    }

    private static async Task CreateSetConfigFlag()
    {
        if (s_fileSystemWrapper.ExistsFile(s_setConfigFlag))
            return;

        await s_fileSystemWrapper.WriteAllTextToFile(s_setConfigFlag, string.Empty);
    }

    private static void DeleteSetConfigFlag()
    {
        if (!s_fileSystemWrapper.ExistsFile(s_setConfigFlag))
            return;

        s_fileSystemWrapper.DeleteFile(s_setConfigFlag);
    }

    private static bool TryInitializeService()
    {
        if (EnvironmentVariables.HasInvalidVariables)
        {
            LogEnvironmentVariablesInvalid(
                s_logger,
                Environment.NewLine +
                string.Join(Environment.NewLine, EnvironmentVariables.InvalidVariables));

            return false;
        }

        ResourceManager.Initialize(s_logger);
        return true;
    }

    [LoggerMessage(LogLevel.Information, "Creating system configuration started.")]
    private static partial void LogCreateSystemConfigurationStarted(ILogger logger);

    [LoggerMessage(LogLevel.Error, "The following environment variables were not specified or are invalid: {InnerMessage}")]
    private static partial void LogEnvironmentVariablesInvalid(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Information, "Found existing system configuration '{InnerMessage}'.")]
    private static partial void LogFoundExistingSystemConfiguration(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Information, "Preparing configuration at boot time started.")]
    private static partial void LogPrepareConfigurationAtBootTime(ILogger logger);

    [LoggerMessage(LogLevel.Error, "An error occurred while running the service. {InnerMessage}")]
    private static partial void LogServiceError(ILogger logger, string? innerMessage);

    // General logs
    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogResultReturnedError(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void LogResultReturnedWarning(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Information, "{InnerMessage}")]
    private static partial void LogResultReturnedSuccess(ILogger logger, string? innerMessage);
}
