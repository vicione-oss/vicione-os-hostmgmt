using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Contains strongly typed named pipe request descriptors for communication contracts whose JSON metadata is
/// provided by <see cref="SourceGenerationContext"/>.
/// </summary>
public static class PipeRequests
{
    /// <summary>Gets the current system configuration.</summary>
    public static PipeRequest<GetSystemConfigurationResult> GetSystemConfiguration { get; } = new(
        Topics.GetSystemConfiguration,
        SourceGenerationContext.Default.GetSystemConfigurationResult);

    /// <summary>Sets the system configuration.</summary>
    public static PipeRequest<SetSystemConfigurationResult> SetSystemConfiguration { get; } = new(
        Topics.SetSystemConfiguration,
        SourceGenerationContext.Default.SetSystemConfigurationResult);

    /// <summary>Gets the set of supported capabilities.</summary>
    public static PipeRequest<GetSupportedCapabilitiesResult> GetSupportedCapabilities { get; } = new(
        Topics.GetSupportedCapabilities,
        SourceGenerationContext.Default.GetSupportedCapabilitiesResult);

    /// <summary>Gets the original physical address.</summary>
    public static PipeRequest<GetOriginalPhysicalAddressResult> GetOriginalPhysicalAddress { get; } = new(
        Topics.GetOriginalPhysicalAddress,
        SourceGenerationContext.Default.GetOriginalPhysicalAddressResult);

    /// <summary>Gets network status information.</summary>
    public static PipeRequest<GetNetworkStatusInformationResult> GetNetworkStatusInformation { get; } = new(
        Topics.GetNetworkStatusInformation,
        SourceGenerationContext.Default.GetNetworkStatusInformationResult);

    /// <summary>Gets DHCP lease information.</summary>
    public static PipeRequest<GetDHCPLeaseInformationResult> GetDHCPLeaseInformation { get; } = new(
        Topics.GetDHCPLeaseInformation,
        SourceGenerationContext.Default.GetDHCPLeaseInformationResult);

    /// <summary>Renews the DHCP lease.</summary>
    public static PipeRequest<GetDHCPLeaseInformationResult> RenewDHCPLease { get; } = new(
        Topics.RenewDHCPLease,
        SourceGenerationContext.Default.GetDHCPLeaseInformationResult);

    /// <summary>Gets NTP fallback information.</summary>
    public static PipeRequest<GetNTPFallbackInformationResult> GetNTPFallbackInformation { get; } = new(
        Topics.GetNTPFallbackInformation,
        SourceGenerationContext.Default.GetNTPFallbackInformationResult);

    /// <summary>Gets a service state.</summary>
    public static PipeRequest<GetServiceStateResult> GetServiceState { get; } = new(
        Topics.GetServiceState,
        SourceGenerationContext.Default.GetServiceStateResult);

    /// <summary>Redeploys application scripts.</summary>
    public static PipeRequest<RedeployScriptsResult> RedeployScripts { get; } = new(
        Topics.RedeployScripts,
        SourceGenerationContext.Default.RedeployScriptsResult);

    /// <summary>Starts a service.</summary>
    public static PipeRequest<ServiceControlResult> StartService { get; } = new(
        Topics.StartService,
        SourceGenerationContext.Default.ServiceControlResult);

    /// <summary>Stops a service.</summary>
    public static PipeRequest<ServiceControlResult> StopService { get; } = new(
        Topics.StopService,
        SourceGenerationContext.Default.ServiceControlResult);

    /// <summary>Restarts a service.</summary>
    public static PipeRequest<ServiceControlResult> RestartService { get; } = new(
        Topics.RestartService,
        SourceGenerationContext.Default.ServiceControlResult);

    /// <summary>Resets a network interface.</summary>
    public static PipeRequest<SystemControlResult> ResetNetworkInterface { get; } = new(
        Topics.ResetNetworkInterface,
        SourceGenerationContext.Default.SystemControlResult);

    /// <summary>Installs a signed Debian package.</summary>
    public static PipeRequest<SystemControlResult> InstallSignedDebianPackage { get; } = new(
        Topics.InstallSignedDebianPackage,
        SourceGenerationContext.Default.SystemControlResult);

    /// <summary>Resets the system.</summary>
    public static PipeRequest<SystemControlResult> ResetSystem { get; } = new(
        Topics.ResetSystem,
        SourceGenerationContext.Default.SystemControlResult);

    /// <summary>Restarts the system.</summary>
    public static PipeRequest<SystemControlResult> RestartSystem { get; } = new(
        Topics.RestartSystem,
        SourceGenerationContext.Default.SystemControlResult);

    /// <summary>Shuts down the system.</summary>
    public static PipeRequest<SystemControlResult> ShutdownSystem { get; } = new(
        Topics.ShutdownSystem,
        SourceGenerationContext.Default.SystemControlResult);

    /// <summary>Updates the system.</summary>
    public static PipeRequest<SystemControlResult> UpdateSystem { get; } = new(
        Topics.UpdateSystem,
        SourceGenerationContext.Default.SystemControlResult);
}
