using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
            CliStrings.Common.ModifyCommandName,
            CliStrings.Network.Interface.IPv4.ModifyCommandDescription)
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
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        foreach (var ipv4Detail in interfaceToConfigure.IPv4.IPv4Details)
        {
            if (ipv4Detail.IPAddress.Equals(newAddress) &&
                ipv4Detail.Netmask.Equals(newNetmask))
            {
                SimpleOutput.WriteLineInformation(
                    CliStrings.Network.Interface.IP.AlreadyDefined,
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
                    SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.DHCP.IsEnabled, interfaceName);

                SimpleOutput.WriteLine(
                    CliStrings.Network.Interface.IP.Modified,
                    interfaceName,
                    oldAddress,
                    oldNetmask,
                    newAddress,
                    newNetmask);

                return;
            }
        }

        SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.IP.NotFound, oldAddress, interfaceName);
    }
}
