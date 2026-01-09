using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;

namespace HostManagement.NetworkStatus;

/// <summary>
/// Represents a network status information script handler.
/// </summary>
internal interface INetworkStatusInformationScriptHandler
{
    /// <summary>
    /// Executes the script to get the network status information.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and <see cref="NetworkStatusInformation"/>.</returns>
    Task<(OperationStatus, NetworkStatusInformation)> ExecuteGetNetworkStatusInformationScript();
}
