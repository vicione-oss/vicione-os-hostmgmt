using HostManagement.Scripts.ScriptOptionNames;
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
    /// <param name="interfaceName">The interface name to get network status information for.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="OperationStatus"/> and the <see cref="NetworkStatusInformation"/>.</returns>
    internal async Task<(OperationStatus, NetworkStatusInformation)> GetNetworkStatusInformation(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            GetNetworkStatusInformationOptionName.InterfaceName,
            interfaceName);

        return await _scriptHandler.ExecuteGetNetworkStatusInformationScript(scriptOptions);
    }
}
