using System.Diagnostics;
using System.IO.Abstractions;
using HostManagement.Communication.NamedPipes;
using HostManagement.Communication.NamedPipes.Extensions;
using HostManagement.Configuration;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig;
using Microsoft.Extensions.Logging;

namespace HostManagement;

/// <summary>
/// Represents the <see cref="ServiceRunner"/>, which is responsible for starting the service or
/// preparing the configuration at boot time.
/// </summary>
internal static partial class ServiceRunner
{
    private static readonly ILogger s_logger = new ConsoleLogger(EnvironmentVariables.LogLevel.Value);
    private static readonly FileSystem s_fileSystem = new();
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

            if (EnvironmentVariables.Environment.Value is TargetEnvironment.EdgeSViciOne)
            {
                // 'PrepareConfiguration' should only be called on supported target environments, like 'edges-vicione'.
                // However, if it is called on a not supported environment, like 'debian' or 'unknown',
                // prepare configuration will not be executed.

                PrepareConfigurationStarted(s_logger);

                var configurationService = new SystemConfigurationService(s_fileSystem, new SystemConfigurationPersistence(s_fileSystem), new ScriptHandler());

                var prepareConfigResult = await configurationService.PrepareConfiguration();

                if (prepareConfigResult.Status == OperationStatus.Error)
                {
                    ResultReturnedError(s_logger, prepareConfigResult.Message);

                    // gracefully exit application
                    return 0;
                }

                await CreateSetConfigFlag();

                if (prepareConfigResult.Status == OperationStatus.Warning)
                    ResultReturnedWarning(s_logger, prepareConfigResult.Message);

                if (prepareConfigResult.Status == OperationStatus.Success)
                    ResultReturnedSuccess(s_logger, prepareConfigResult.Message);
            }

            return 0;
        }
        catch (Exception ex)
        {
            ServiceErrorOccurredWhileRunning(s_logger, ex);

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
            using ConsoleLifetime lifetime = new(s_logger);

            if (await IsServiceAlreadyRunning(lifetime.Token))
                return 1;

            if (!TryInitializeService())
                return 1;

            if (s_fileSystem.File.Exists(s_setConfigFlag))
            {
                s_fileSystem.File.Delete(s_setConfigFlag);

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
            ServiceErrorOccurredWhileRunning(s_logger, ex);
            return 1;
        }
    }

    private static async Task<GetSystemConfigurationResult> GetJsonConfigurationOnStartUp()
    {
        var persistence = new SystemConfigurationPersistence(s_fileSystem);

        if (persistence.UserConfigurationExists())
            FoundExistingSystemConfiguration(s_logger, persistence.GetUserConfigurationPath());
        else if (persistence.PreparationConfigurationExists())
            FoundExistingSystemConfiguration(s_logger, persistence.GetPreparationConfigurationPath());
        else
            CreateSystemConfigurationStarted(s_logger);

        var configurationService = new SystemConfigurationService(s_fileSystem, persistence, new ScriptHandler());
        var getJsonConfigResult = await configurationService.GetConfigurationFromJson();

        if (getJsonConfigResult.Status == OperationStatus.Error)
            ResultReturnedError(s_logger, getJsonConfigResult.Message);

        if (getJsonConfigResult.Status == OperationStatus.Warning)
            ResultReturnedWarning(s_logger, getJsonConfigResult.Message);

        if (getJsonConfigResult.Status == OperationStatus.Success)
            ResultReturnedSuccess(s_logger, getJsonConfigResult.Message);

        return getJsonConfigResult;
    }

    private static async Task<bool> SetJsonConfigurationOnStartUp(
        SystemConfiguration jsonConfig,
        CancellationToken cancellationToken)
    {
        var configurationService = new SystemConfigurationService(s_fileSystem, new SystemConfigurationPersistence(s_fileSystem), new ScriptHandler());
        var setConfigResult = await configurationService
            .SetConfiguration(jsonConfig, shouldPersistConfiguration: false, cancellationToken);

        if (setConfigResult.Status == OperationStatus.Error)
        {
            ResultReturnedError(s_logger, setConfigResult.Message);
            return false;
        }

        if (setConfigResult.Status == OperationStatus.Warning)
            ResultReturnedWarning(s_logger, setConfigResult.Message);

        if (setConfigResult.Status == OperationStatus.Success)
            ResultReturnedSuccess(s_logger, setConfigResult.Message);

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
        if (!s_fileSystem.File.Exists(s_setConfigFlag))
            await s_fileSystem.File.Create(s_setConfigFlag).DisposeAsync();
    }

    private static bool TryInitializeService()
    {
        if (EnvironmentVariables.HasInvalidVariables)
        {
            EnvironmentVariablesInvalid(
                s_logger,
                Environment.NewLine +
                string.Join(Environment.NewLine, EnvironmentVariables.InvalidVariables));

            return false;
        }

        var environmentString = EnvironmentVariables.Environment.Value.ToEnvironmentString();
        var resourceDeployDirectory = EnvironmentVariables.ResourceDeployDirectory.Value;

        CurrentTargetEnvironment(s_logger, environmentString);

        var resourceManagerInstance = ResourceManager.Initialize(
            s_logger,
            s_fileSystem,
            resourceDeployDirectory,
            environmentString);

        ScriptRunner.Initialize(resourceDeployDirectory);

        return resourceManagerInstance.DeployScripts();
    }

    private static async Task<bool> IsServiceAlreadyRunning(CancellationToken cancellationToken)
    {
        const string ServiceName = "hostmanagement";
        const string ServiceNameAndDaemonFlag = $"{ServiceName} --daemon";
        const string PgrepCommand = $"pgrep {ServiceName} -a";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = @"/bin/bash",
                ArgumentList = { "-c", PgrepCommand },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        process.Start();

        await process.WaitForExitAsync(cancellationToken);

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            CouldNotDetermineServiceAlreadyRunning(s_logger, error);

            // Assume a service is already running
            return true;
        }

        if (string.IsNullOrWhiteSpace(output))
            return false;

        var serviceProcesses = output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(pidAndCommandLine => pidAndCommandLine.Contains(ServiceNameAndDaemonFlag))
            .ToList();

        if (serviceProcesses.Count == 1)
        {
            // If only one process is returned, it must be the current instance
            return false;
        }

        ServiceAlreadyRunning(s_logger);
        return true;
    }

    [LoggerMessage(LogLevel.Error, "An error occurred while starting the service. Could not determine if the service is already running. {StandardError}")]
    private static partial void CouldNotDetermineServiceAlreadyRunning(ILogger logger, string? standardError);

    [LoggerMessage(LogLevel.Information, "Creating system configuration started.")]
    private static partial void CreateSystemConfigurationStarted(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Current target environment: '{TargetEnvironment}'")]
    private static partial void CurrentTargetEnvironment(ILogger logger, string targetEnvironment);

    [LoggerMessage(LogLevel.Error, "The following environment variables were not specified or are invalid: {EnvironmentVariables}")]
    private static partial void EnvironmentVariablesInvalid(ILogger logger, string? environmentVariables);

    [LoggerMessage(LogLevel.Information, "Found existing system configuration '{SystemConfigurationFilePath}'.")]
    private static partial void FoundExistingSystemConfiguration(ILogger logger, string? systemConfigurationFilePath);

    [LoggerMessage(LogLevel.Information, "Preparing configuration at boot time started.")]
    private static partial void PrepareConfigurationStarted(ILogger logger);

    [LoggerMessage(LogLevel.Error, "{Message}")]
    private static partial void ResultReturnedError(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Warning, "{Message}")]
    private static partial void ResultReturnedWarning(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Information, "{Message}")]
    private static partial void ResultReturnedSuccess(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Error, "An error occurred while starting the service. The service is already running.")]
    private static partial void ServiceAlreadyRunning(ILogger logger);

    [LoggerMessage(LogLevel.Error, "An error occurred while running the service.")]
    private static partial void ServiceErrorOccurredWhileRunning(ILogger logger, Exception exception);
}
