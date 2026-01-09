using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;

namespace HostManagement.NetworkStatus;

/// <summary>
/// Represents the network status information adapter.
/// </summary>
internal sealed class NetworkStatusInformationAdapter
{
    private readonly INetworkStatusInformationScriptHandler _scriptHandler;

    /// <summary>
    /// Initializes a new <see cref="NetworkStatusInformationAdapter"/>.
    /// </summary>
    /// <param name="scriptHandler">An <see cref="INetworkStatusInformationScriptHandler"/>
    /// implementation.</param>
    public NetworkStatusInformationAdapter(INetworkStatusInformationScriptHandler scriptHandler)
    {
        _scriptHandler = scriptHandler;
    }

    /// <summary>
    /// Gets the network status information.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and the <see cref="NetworkStatusInformation"/>.</returns>
    internal async Task<(OperationStatus, NetworkStatusInformation)> GetNetworkStatusInformation(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _scriptHandler.ExecuteGetNetworkStatusInformationScript();
    }
}
