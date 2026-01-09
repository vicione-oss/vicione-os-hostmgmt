using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
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
    /// <param name="interfaceNameOption">The interface name option.</param>
    public DHCPRenewLeaseCommand(Option<string> interfaceNameOption) : base(
        CommandLineStrings_NetworkInterfaces.DHCPRenewLeaseCommandName,
        CommandLineStrings_NetworkInterfaces.DHCPRenewLeaseCommandDescription)
    {
        Add(interfaceNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return RenewLeaseHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                cancellationToken);
        });
    }

    private static async Task RenewLeaseHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var output = new KeyValueOutput();

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            output
                .WithWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName)
                .WriteLine();

            return;
        }

        if (!interfaceToConfigure.IPv4.DHCPEnabled)
        {
            output
                .WithInformation(CommandLineStrings_NetworkInterfaces.DHCPIsDisabled, interfaceName)
                .WriteLine();

            return;
        }

        if (await NamedPipeRequestHelper.RenewDHCPLease(interfaceName, cancellationToken) is not DHCPLease dhcpLease)
            return;

        output
            .WithTitle(CommandLineStrings_NetworkInterfaces.DHCPLeaseRenewed)
            .With(new(CommandLineStrings_NetworkInterfaces.DHCPLeaseObtained, dhcpLease.LeaseObtained))
            .With(new(CommandLineStrings_NetworkInterfaces.DHCPLeaseExpires, dhcpLease.LeaseExpires))
            .With(new(CommandLineStrings_NetworkInterfaces.Gateway, dhcpLease.Gateway))
            .With(new(CommandLineStrings_NetworkInterfaces.IPAddress, dhcpLease.IPv4Detail?.IPAddress))
            .With(new(CommandLineStrings_NetworkInterfaces.SubnetMask, dhcpLease.IPv4Detail?.Netmask))
            .WriteLine();
    }
}
