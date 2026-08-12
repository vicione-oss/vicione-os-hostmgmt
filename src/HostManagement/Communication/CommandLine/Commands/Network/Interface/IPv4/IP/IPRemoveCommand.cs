using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;

/// <summary>
/// Represents the IP remove command.
/// </summary>
internal sealed class IPRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="IPRemoveCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    /// <param name="addressOption">The IP address option.</param>
    public IPRemoveCommand(Option<string> interfaceNameOption, Option<IPAddress> addressOption) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_NetworkInterfaces.IPRemoveCommandDescription)
    {
        Add(interfaceNameOption);
        Add(addressOption);

        SetAction((parseResult, cancellationToken)
            => RemoveHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                parseResult.GetRequiredValue(addressOption),
                cancellationToken));
    }

    private static async Task RemoveHandler(
        string interfaceName,
        IPAddress ipAddress,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        var ipv4DetailToRemove = interfaceToConfigure.IPv4.IPv4Details
            .FirstOrDefault(ipv4Detail => ipv4Detail.IPAddress.Equals(ipAddress));

        if (ipv4DetailToRemove is null)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceIPNotFound, ipAddress, interfaceName);
            return;
        }

        interfaceToConfigure.IPv4.IPv4Details.Remove(ipv4DetailToRemove);

        var output = new ListOutput();

        if (interfaceToConfigure.IPv4.IPv4Details.Count == 0)
        {
            if (interfaceToConfigure.IPv4.Gateway is not null)
            {
                var removedGateway = interfaceToConfigure.IPv4.Gateway;
                interfaceToConfigure.IPv4.Gateway = null;

                output.WithInformation(CommandLineStrings_NetworkInterfaces.GatewayRemoved, removedGateway);
            }

            if (!interfaceToConfigure.IPv4.DHCPEnabled)
            {
                interfaceToConfigure.IPv4.DHCPEnabled = true;
                output.WithInformation(CommandLineStrings_NetworkInterfaces.DHCPEnabled, interfaceName);
            }
        }
        else if (interfaceToConfigure.IPv4.DHCPEnabled)
        {
            output.WithWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (output.HasMessages)
            output.WriteLine();

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.InterfaceIPRemoved, ipAddress, interfaceName);
    }
}
