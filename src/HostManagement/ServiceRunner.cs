using System.Diagnostics;
using System.IO.Abstractions;
using HostManagement.Communication.NamedPipe.Extensions;
using HostManagement.Communication.NamedPipe.Server;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Scripts;
using HostManagement.Services;
using HostManagement.Shared.Communication.Capabilities;
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
            using var initializationContext = await TryInitializeService(CancellationToken.None).ConfigureAwait(false);

            if (initializationContext is null)
                return 0;

            if (EnvironmentVariables.Environment.Value is not TargetEnvironment.EdgeSViciOne)
                return 0;

            // 'PrepareConfiguration' should only be called on supported target environments, like 'edges-vicione'.
            // However, if it is called on a not supported environment, like 'debian' or 'unknown',
            // prepare configuration will not be executed.

            var status = await initializationContext.ConfigurationService
                .PrepareConfigurationOnDeviceStartup(CancellationToken.None)
                .ConfigureAwait(false);

            if (status is OperationStatus.Success)
                await CreateSetConfigFlag().ConfigureAwait(false);

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

            if (await IsServiceAlreadyRunning(lifetime.Token).ConfigureAwait(false))
                return 1;

            using var initializationContext = await TryInitializeService(lifetime.Token).ConfigureAwait(false);

            if (initializationContext is null)
                return 1;

            if (s_fileSystem.File.Exists(s_setConfigFlag))
            {
                DeleteSetConfigFlag();

                var configResult = await initializationContext.ConfigurationService
                    .GetConfigurationOnDeviceStartup(lifetime.Token)
                    .ConfigureAwait(false);

                if (configResult.Configuration is SystemConfiguration jsonConfig)
                {
                    var status = await initializationContext.ConfigurationService
                        .SetConfigurationOnDeviceStartup(jsonConfig, lifetime.Token)
                        .ConfigureAwait(false);

                    if (status == OperationStatus.Error)
                        return 1;
                }
            }

            await StartNamedPipeServer(initializationContext, lifetime.Token).ConfigureAwait(false);
            return 0;
        }
        catch (Exception ex)
        {
            ServiceErrorOccurredWhileRunning(s_logger, ex);
            return 1;
        }
    }

    private static async Task StartNamedPipeServer(
        InitializationContext initializationContext,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var registry = GetRequestHandlerRegistry(initializationContext);

                using var pipeServer = new PipeServer("HostPipe", s_logger, registry);
#if DEBUG
                // Testing purposes
                registry.RegisterHandler(new EchoRequestHandler(pipeServer, s_logger));
#endif
                await pipeServer.SetExecutePermission().ConfigureAwait(false);
                await pipeServer.SetOwnership().ConfigureAwait(false);
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
            await s_fileSystem.File.Create(s_setConfigFlag).DisposeAsync().ConfigureAwait(false);
            CreateSetConfigFlag(s_logger, s_setConfigFlag);
        }
    }

    private static void DeleteSetConfigFlag()
    {
        s_fileSystem.File.Delete(s_setConfigFlag);
        DeletedSetConfigFlag(s_logger, s_setConfigFlag);
    }

    private static async Task<InitializationContext?> TryInitializeService(CancellationToken cancellationToken)
    {
        if (EnvironmentVariables.HasInvalidVariables)
        {
            EnvironmentVariablesInvalid(s_logger, EnvironmentVariables.FormattedInvalidVariables);
            return null;
        }

        var environmentString = EnvironmentVariables.Environment.Value.ToEnvironmentString();
        var deployDirectory = EnvironmentVariables.ResourceDeployDirectory.Value;

        CurrentTargetEnvironment(s_logger, environmentString);

        var resourceManager = ResourceManager.Initialize(s_logger, s_fileSystem, deployDirectory, environmentString);
        _ = ScriptRunner.Initialize(deployDirectory);

        if (!resourceManager.DeployScripts())
            return null;

#pragma warning disable CA2000 // Ownership is transferred to InitializationContext on successful initialization.
        var capabilitiesService = new SupportedCapabilitiesService(s_fileSystem, s_logger);
#pragma warning restore CA2000

        try
        {
            var supportedCapabilities = await capabilitiesService
                .InitializeSupportedCapabilities(cancellationToken)
                .ConfigureAwait(false);

            var configurationService = new SystemConfigurationService(
                new SystemConfigurationPersistence(s_fileSystem),
                new ScriptHandler(),
                s_logger,
                supportedCapabilities.Settings);

            return new(capabilitiesService, supportedCapabilities, configurationService);
        }
        catch
        {
            capabilitiesService.Dispose();
            throw;
        }
    }

    private static async Task<bool> IsServiceAlreadyRunning(CancellationToken cancellationToken)
    {
        const string ServiceName = "hostmanagement";
        const string ServiceNameAndDaemonFlag = $"{ServiceName} --daemon";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pgrep",
                ArgumentList = { "-a", ServiceName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var waitForExitTask = process.WaitForExitAsync(cancellationToken);

        await Task
            .WhenAll(standardOutputTask, standardErrorTask, waitForExitTask)
            .ConfigureAwait(false);

        var standardOutput = (await standardOutputTask.ConfigureAwait(false)).TrimEnd();
        var standardError = (await standardErrorTask.ConfigureAwait(false)).TrimEnd();

        if (process.ExitCode != 0)
        {
            // Assume a service is already running
            CouldNotDetermineServiceAlreadyRunning(s_logger, standardError);
            return true;
        }

        if (string.IsNullOrWhiteSpace(standardOutput))
            return false;

        var serviceProcesses = standardOutput
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

    private static RequestHandlerRegistry GetRequestHandlerRegistry(InitializationContext initializationContext)
    {
        List<IRequestHandler> handlers =
        [
            new GetSupportedCapabilitiesRequestHandler(initializationContext.CapabilitiesService),
            new InstallSignedDebianPackageRequestHandler(s_logger),
            new RedeployScriptsRequestHandler(),
            new RenewDHCPLeaseRequestHandler(s_logger),
            .. GetServiceRequestHandlers(),
            .. GetStateRequestHandlers(),
            .. GetSystemConfigurationRequestHandlers(initializationContext.ConfigurationService),
            .. GetSystemRequestHandlers()
        ];

        return new RequestHandlerRegistry(
            EnvironmentVariables.Environment.Value,
            initializationContext.SupportedCapabilities.Topics,
            handlers,
            s_logger);


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

        static IEnumerable<IRequestHandler> GetSystemConfigurationRequestHandlers(
            SystemConfigurationService configurationService)
        {
            yield return new GetSystemConfigurationRequestHandler(configurationService);
            yield return new SetSystemConfigurationRequestHandler(configurationService);
            yield return new ResetNetworkInterfaceRequestHandler(configurationService);
        }

        static IEnumerable<IRequestHandler> GetSystemRequestHandlers()
        {
            yield return new ResetSystemRequestHandler(s_logger);
            yield return new RestartSystemRequestHandler(s_logger);
            yield return new ShutdownSystemRequestHandler(s_logger);
            yield return new UpdateSystemRequestHandler(s_logger);
        }
    }

    private sealed class InitializationContext(
        SupportedCapabilitiesService capabilitiesService,
        SupportedCapabilities supportedCapabilities,
        SystemConfigurationService configurationService)
            : IDisposable
    {
        public SupportedCapabilitiesService CapabilitiesService { get; } = capabilitiesService;

        public SupportedCapabilities SupportedCapabilities { get; } = supportedCapabilities;

        public SystemConfigurationService ConfigurationService { get; } = configurationService;

        public void Dispose() => CapabilitiesService.Dispose();
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
