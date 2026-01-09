using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.RenewDHCPLease;

/// <summary>
/// Represents the renew DHCP lease adapter.
/// </summary>
internal sealed class RenewDHCPLeaseAdapter
{
    private readonly IScriptHandler _scriptHandler;

    /// <summary>
    /// Initializes a new <see cref="RenewDHCPLeaseAdapter"/>.
    /// </summary>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    public RenewDHCPLeaseAdapter(IScriptHandler scriptHandler)
    {
        _scriptHandler = scriptHandler;
    }

    /// <summary>
    /// Renews the DHCP lease of the specified network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease shall be renewed.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="OperationStatus"/> and the <see cref="DHCPLease"/>.</returns>
    internal async Task<(OperationStatus, DHCPLease)> RenewDHCPLease(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var status = await _scriptHandler.ExecuteRenewDHCPLeaseScript(interfaceName);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, DHCPLease.Empty);

        await Task.Delay(1000, cancellationToken);

        (status, var dhcpLease) = await _scriptHandler.ExecuteGetInterfaceIPv4DHCPLeaseScript(interfaceName);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, DHCPLease.Empty);

        return (OperationStatus.Success, dhcpLease);
    }
}
