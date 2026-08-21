using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;

/// <summary>
/// Represents the DHCP renew-lease command.
/// </summary>
internal sealed class DHCPRenewLeaseCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DHCPRenewLeaseCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public DHCPRenewLeaseCommand(Argument<string> interfaceNameArgument)
        : base(
            CliStrings.Network.Interface.DHCP.RenewLeaseCommandName,
            CliStrings.Network.Interface.DHCP.RenewLeaseCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => RenewLeaseHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task RenewLeaseHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        if (!interfaceToConfigure.IPv4.DHCPEnabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Interface.DHCP.IsDisabled, interfaceName);
            return;
        }

        if (await NamedPipeRequestHelper.RenewDHCPLease(interfaceName, cancellationToken) is not DHCPLease dhcpLease)
            return;

        new KeyValueOutput()
            .WithTitle(CliStrings.Network.Interface.DHCP.LeaseRenewed)
            .With(new(CliStrings.Network.Interface.DHCP.LeaseObtained, dhcpLease.LeaseObtained))
            .With(new(CliStrings.Network.Interface.DHCP.LeaseExpires, dhcpLease.LeaseExpires))
            .With(new(CliStrings.Network.Interface.Gateway.Label, dhcpLease.Gateway))
            .With(new(CliStrings.Network.Interface.IP.IPAddress, dhcpLease.IPv4Detail?.IPAddress))
            .With(new(CliStrings.Network.Interface.IP.SubnetMask, dhcpLease.IPv4Detail?.Netmask))
            .WriteLine();
    }
}
