using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;

/// <summary>
/// Represents the IP add command.
/// </summary>
internal sealed class IPAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="IPAddCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    /// <param name="addressOption">The IP address option.</param>
    /// <param name="netmaskOption">The netmask option.</param>
    public IPAddCommand(
        Option<string> interfaceNameOption,
        Option<IPAddress> addressOption,
        Option<IPAddress> netmaskOption)
        : base(
            CliStrings.Common.AddCommandName,
            CliStrings.Network.Interface.IPv4.AddCommandDescription)
    {
        Add(interfaceNameOption);
        Add(addressOption);
        Add(netmaskOption);

        SetAction((parseResult, cancellationToken)
            => AddHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                parseResult.GetRequiredValue(addressOption),
                parseResult.GetRequiredValue(netmaskOption),
                cancellationToken));
    }

    private static async Task AddHandler(
        string interfaceName,
        IPAddress ipAddress,
        IPAddress netmask,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        interfaceToConfigure.IPv4.IPv4Details.Add(new IPv4Detail()
        {
            IPAddress = ipAddress,
            Netmask = netmask
        });

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (interfaceToConfigure.IPv4.DHCPEnabled)
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.DHCP.IsEnabled, interfaceName);

        SimpleOutput.WriteLine(CliStrings.Network.Interface.IP.Added, ipAddress, netmask, interfaceName);
    }
}
