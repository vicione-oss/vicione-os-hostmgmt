using System.Diagnostics;
using System.IO.Abstractions;
using HostManagement.Communication.NamedPipes;
using HostManagement.Communication.NamedPipes.Extensions;
using HostManagement.Communication.NamedPipes.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Scripts;
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
    private static readonly SystemConfigurationPersistence s_systemConfigurationPersistence = new(s_fileSystem);
    private static readonly SystemConfigurationService s_configurationService = new(
        s_systemConfigurationPersistence,
        new ScriptHandler(),
        s_logger);

    private static readonly string s_setConfigFlag = Path.Combine(AppPaths.TempPath, "set-config.flag");

    /// <summary>
    /// Starts the "prepare configuration" mode of the application, that runs at device startup.
    /// </summary>
    /// <remarks>
    /// Prepares the system by writing changes of the '<c>SystemConfiguration.json</c>' or
    /// '<c>DefaultSystemConfiguration.json</c>' to the system without applying them yet and
    /// creates a '<c>set-config.flag</c>' file if successful.
    /// </remarks>
    /// <returns>Always returns 0 (graceful exit on error), so service start does not fail.</returns>
    internal static async Task<int> PrepareConfiguration()
    {
        try
        {
            if (!TryInitializeService())
                return 0;

            if (EnvironmentVariables.Environment.Value is TargetEnvironment.EdgeSViciOne)
            {
                // 'PrepareConfiguration' should only be called on supported target environments, like 'edges-vicione'.
                // However, if it is called on a not supported environment, like 'debian' or 'unknown',
                // prepare configuration will not be executed.

                var status = await s_configurationService
                    .PrepareConfigurationOnDeviceStartup(CancellationToken.None)
                    .ConfigureAwait(false);

                if (status == OperationStatus.Warning)
                    return 0;

                await CreateSetConfigFlag();
            }

            return 0;
        }
        catch (Exception ex)
        {
            ServiceErrorOccurredWhileRunning(s_logger, ex);
            return 0;
        }
    }

    /// <summary>
    /// Starts the application as a service, that runs in the background.
    /// </summary>
    /// <remarks>
    /// If '<c>set-config.flag</c>' is present, either '<c>SystemConfiguration.json</c>' or
    /// '<c>DefaultSystemConfiguration.json</c>' is applied. The flag file is deleted.
    /// </remarks>
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
                DeleteSetConfigFlag();

                var configResult = await s_configurationService.GetConfigurationOnDeviceStartup(lifetime.Token)
                    .ConfigureAwait(false);

                if (configResult.Configuration is SystemConfiguration jsonConfig)
                {
                    var status = await s_configurationService
                        .SetConfigurationOnDeviceStartup(jsonConfig, lifetime.Token)
                        .ConfigureAwait(false);

                    if (status == OperationStatus.Error)
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

    private static async Task StartNamedPipeServer(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var registry = GetRequestHandlerRegistry();

                using var pipeServer = new PipeServer("HostPipe", s_logger, registry);
#if DEBUG
                // Testing purposes
                registry.RegisterHandler(new EchoRequestHandler(pipeServer, s_logger));
#endif

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
        {
            await s_fileSystem.File.Create(s_setConfigFlag).DisposeAsync();
            CreateSetConfigFlag(s_logger, s_setConfigFlag);
        }
    }

    private static void DeleteSetConfigFlag()
    {
        s_fileSystem.File.Delete(s_setConfigFlag);
        DeletedSetConfigFlag(s_logger, s_setConfigFlag);
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

    private static RequestHandlerRegistry GetRequestHandlerRegistry()
    {
        var registry = new RequestHandlerRegistry(EnvironmentVariables.Environment.Value);
        registry.RegisterHandler(new InstallSignedDebianPackageRequestHandler(s_logger));
        registry.RegisterHandler(new RedeployScriptsRequestHandler());
        registry.RegisterHandler(new RenewDHCPLeaseRequestHandler(s_logger));
        registry.RegisterHandlers(GetServiceRequestHandlers());
        registry.RegisterHandlers(GetStateRequestHandlers());
        registry.RegisterHandlers(GetSystemConfigurationRequestHandlers());
        registry.RegisterHandlers(GetSystemRequestHandlers());

        return registry;


        static IEnumerable<IRequestHandler> GetServiceRequestHandlers()
        {
            yield return new GetServiceStateRequestHandler(s_logger);
            yield return new StartServiceRequestHandler(s_logger);
            yield return new StopServiceRequestHandler(s_logger);
            yield return new RestartServiceRequestHandler(s_logger);
        }

        static IEnumerable<IRequestHandler> GetStateRequestHandlers()
        {
            yield return new GetOriginalPhysicalAddressRequestHandler(s_logger);
            yield return new GetNetworkStatusInformationRequestHandler(s_logger);
            yield return new GetDHCPLeaseInformationRequestHandler(s_logger);
            yield return new GetNTPFallbackInformationRequestHandler(s_logger);
        }

        static IEnumerable<IRequestHandler> GetSystemConfigurationRequestHandlers()
        {
            yield return new GetSystemConfigurationRequestHandler(s_configurationService);
            yield return new SetSystemConfigurationRequestHandler(s_configurationService);
            yield return new ResetNetworkInterfaceRequestHandler(s_configurationService);
        }

        static IEnumerable<IRequestHandler> GetSystemRequestHandlers()
        {
            yield return new ResetSystemRequestHandler(s_logger);
            yield return new RestartSystemRequestHandler(s_logger);
            yield return new ShutdownSystemRequestHandler(s_logger);
            yield return new UpdateSystemRequestHandler(s_logger);
        }
    }

    [LoggerMessage(LogLevel.Error, "An error occurred while starting the service. Could not determine if the service is already running. {StandardError}")]
    private static partial void CouldNotDetermineServiceAlreadyRunning(ILogger logger, string? standardError);

    [LoggerMessage(LogLevel.Debug, "Created flag '{FlagFile}'.")]
    private static partial void CreateSetConfigFlag(ILogger logger, string flagFile);

    [LoggerMessage(LogLevel.Information, "Current target environment: '{TargetEnvironment}'")]
    private static partial void CurrentTargetEnvironment(ILogger logger, string targetEnvironment);

    [LoggerMessage(LogLevel.Debug, "Deleted flag '{FlagFile}'.")]
    private static partial void DeletedSetConfigFlag(ILogger logger, string flagFile);

    [LoggerMessage(LogLevel.Error, "The following environment variables were not specified or are invalid: {EnvironmentVariables}")]
    private static partial void EnvironmentVariablesInvalid(ILogger logger, string? environmentVariables);

    [LoggerMessage(LogLevel.Error, "An error occurred while starting the service. The service is already running.")]
    private static partial void ServiceAlreadyRunning(ILogger logger);

    [LoggerMessage(LogLevel.Error, "An error occurred while running the service.")]
    private static partial void ServiceErrorOccurredWhileRunning(ILogger logger, Exception exception);
}
