using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;

/// <summary>
/// Represents the IP modify command.
/// </summary>
internal sealed class IPModifyCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="IPModifyCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    /// <param name="oldAddressOption">The old IP address option.</param>
    /// <param name="newAddressOption">The new IP address option.</param>
    /// <param name="netmaskOption">The netmask option.</param>
    public IPModifyCommand(
        Option<string> interfaceNameOption,
        Option<IPAddress> oldAddressOption,
        Option<IPAddress> newAddressOption,
        Option<IPAddress> netmaskOption)
        : base(
            CommandLineStrings_Common.ModifyCommandName,
            CommandLineStrings_NetworkInterfaces.IPModifyCommandDescription)
    {
        Add(interfaceNameOption);
        Add(oldAddressOption);
        Add(newAddressOption);
        Add(netmaskOption);

        SetAction((parseResult, cancellationToken)
            => ModifyHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                parseResult.GetRequiredValue(oldAddressOption),
                parseResult.GetRequiredValue(newAddressOption),
                parseResult.GetRequiredValue(netmaskOption),
                cancellationToken));
    }

    private static async Task ModifyHandler(
        string interfaceName,
        IPAddress oldAddress,
        IPAddress newAddress,
        IPAddress newNetmask,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        foreach (var ipv4Detail in interfaceToConfigure.IPv4.IPv4Details)
        {
            if (ipv4Detail.IPAddress.Equals(newAddress) &&
                ipv4Detail.Netmask.Equals(newNetmask))
            {
                SimpleOutput.WriteLineInformation(
                    CommandLineStrings_NetworkInterfaces.InterfaceIPAlreadyDefined,
                    newAddress,
                    newNetmask);

                return;
            }

            if (ipv4Detail.IPAddress.Equals(oldAddress))
            {
                var oldNetmask = ipv4Detail.Netmask;

                ipv4Detail.IPAddress = newAddress;
                ipv4Detail.Netmask = newNetmask;

                if (!await SetConfiguration(config, cancellationToken))
                    return;

                if (interfaceToConfigure.IPv4.DHCPEnabled)
                    SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);

                SimpleOutput.WriteLine(
                    CommandLineStrings_NetworkInterfaces.InterfaceIPModified,
                    interfaceName,
                    oldAddress,
                    oldNetmask,
                    newAddress,
                    newNetmask);

                return;
            }
        }

        SimpleOutput.WriteLineWarning(
            CommandLineStrings_NetworkInterfaces.InterfaceIPNotFound,
            oldAddress,
            interfaceName);
    }
}
