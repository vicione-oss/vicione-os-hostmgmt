using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.Gateway;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;
using HostManagement.Communication.CommandLine.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

/// <summary>
/// Represents the <see cref="IPv4CommandGroupProvider"/>.
/// </summary>
internal static class IPv4CommandGroupProvider
{
    /// <summary>
    /// Adds the command group to configure IPv4 settings of a specified network interface.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddIPv4CommandGroup(this Command parentCommand)
    {
        var ipv4Command = parentCommand.AddCommand(
            "ipv4",
            "Configure IPv4 settings of a specified network interface.");

        ipv4Command.AddEnableDHCPCommandGroup();
        ipv4Command.AddIPCommandGroup();
        ipv4Command.AddGatewayCommandGroup();
        ipv4Command.AddVLANCommandGroup();
    }
}
