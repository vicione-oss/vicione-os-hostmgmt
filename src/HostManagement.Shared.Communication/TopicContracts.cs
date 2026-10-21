using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Contracts.System;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Binds every topic in <see cref="Topics"/> to its request and response contracts.
/// </summary>
/// <remarks>
/// Passing one of these instead of a topic string lets the compiler infer the contract types, so a caller never names
/// a result type or a JsonTypeInfo. Topics whose request content is raw text (or which take no content) are bound with
/// <see cref="TopicContract{TResponse}"/> and keep taking a <see cref="string"/>.
/// </remarks>
public static class TopicContracts
{
    #region Topics to get or set the complete configuration.
    /// <inheritdoc cref="Topics.GetSystemConfiguration"/>
    public static TopicContract<GetSystemConfigurationResult> GetSystemConfiguration { get; }
        = new(Topics.GetSystemConfiguration);

    /// <inheritdoc cref="Topics.SetSystemConfiguration"/>
    public static TopicContract<SystemConfiguration, SetSystemConfigurationResult> SetSystemConfiguration { get; }
        = new(Topics.SetSystemConfiguration);
    #endregion

    #region Topics to get or set a specific configuration section.
    /// <inheritdoc cref="Topics.ResetNetworkInterface"/>
    public static TopicContract<SystemControlResult> ResetNetworkInterface { get; }
        = new(Topics.ResetNetworkInterface);
    #endregion

    #region Topics to retrieve a state or information. This is not part of the configuration.
    /// <inheritdoc cref="Topics.GetSupportedCapabilities"/>
    public static TopicContract<GetSupportedCapabilitiesResult> GetSupportedCapabilities { get; }
        = new(Topics.GetSupportedCapabilities);

    /// <inheritdoc cref="Topics.GetOriginalPhysicalAddress"/>
    public static TopicContract<GetOriginalPhysicalAddressResult> GetOriginalPhysicalAddress { get; }
        = new(Topics.GetOriginalPhysicalAddress);

    /// <inheritdoc cref="Topics.GetNetworkStatusInformation"/>
    public static TopicContract<GetNetworkStatusInformationResult> GetNetworkStatusInformation { get; }
        = new(Topics.GetNetworkStatusInformation);

    /// <inheritdoc cref="Topics.GetDHCPLeaseInformation"/>
    public static TopicContract<GetDHCPLeaseInformationResult> GetDHCPLeaseInformation { get; }
        = new(Topics.GetDHCPLeaseInformation);

    /// <inheritdoc cref="Topics.GetNTPFallbackInformation"/>
    public static TopicContract<GetNTPFallbackInformationResult> GetNTPFallbackInformation { get; }
        = new(Topics.GetNTPFallbackInformation);

    /// <inheritdoc cref="Topics.GetNTPSyncStatus"/>
    public static TopicContract<GetNTPSyncStatusResult> GetNTPSyncStatus { get; }
        = new(Topics.GetNTPSyncStatus);

    /// <inheritdoc cref="Topics.GetServiceState"/>
    public static TopicContract<GetServiceStateResult> GetServiceState { get; }
        = new(Topics.GetServiceState);
    #endregion

    #region Topics to request an action. This is not part of the configuration. The action may result in a new state and/or return data.
    /// <inheritdoc cref="Topics.InstallSignedDebianPackage"/>
    public static TopicContract<SignedDebianPackage, SystemControlResult> InstallSignedDebianPackage { get; }
        = new(Topics.InstallSignedDebianPackage);

    /// <inheritdoc cref="Topics.RedeployScripts"/>
    public static TopicContract<RedeployScriptsResult> RedeployScripts { get; }
        = new(Topics.RedeployScripts);

    /// <inheritdoc cref="Topics.RenewDHCPLease"/>
    public static TopicContract<GetDHCPLeaseInformationResult> RenewDHCPLease { get; }
        = new(Topics.RenewDHCPLease);

    /// <inheritdoc cref="Topics.StartService"/>
    public static TopicContract<ServiceControlRequest, ServiceControlResult> StartService { get; }
        = new(Topics.StartService);

    /// <inheritdoc cref="Topics.StopService"/>
    public static TopicContract<ServiceControlRequest, ServiceControlResult> StopService { get; }
        = new(Topics.StopService);

    /// <inheritdoc cref="Topics.RestartService"/>
    public static TopicContract<ServiceControlRequest, ServiceControlResult> RestartService { get; }
        = new(Topics.RestartService);

    /// <inheritdoc cref="Topics.ResetSystem"/>
    public static TopicContract<SystemControlResult> ResetSystem { get; }
        = new(Topics.ResetSystem);

    /// <inheritdoc cref="Topics.RestartSystem"/>
    public static TopicContract<SystemControlResult> RestartSystem { get; }
        = new(Topics.RestartSystem);

    /// <inheritdoc cref="Topics.ShutdownSystem"/>
    public static TopicContract<SystemControlResult> ShutdownSystem { get; }
        = new(Topics.ShutdownSystem);

    /// <inheritdoc cref="Topics.UpdateSystem"/>
    public static TopicContract<SystemControlResult> UpdateSystem { get; }
        = new(Topics.UpdateSystem);
    #endregion

    #region Topics for testing. This is not part of the configuration.
    /// <inheritdoc cref="Topics.Echo"/>
    /// <remarks>Request and response are raw text.</remarks>
    public static TopicContract<string> Echo { get; } = new(Topics.Echo);
    #endregion
}
