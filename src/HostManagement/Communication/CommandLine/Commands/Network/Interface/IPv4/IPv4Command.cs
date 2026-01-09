using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.Gateway;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.VLAN;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

/// <summary>
/// Represents the IPv4 command.
/// </summary>
internal sealed class IPv4Command : Command
{
    /// <summary>
    /// Initializes a new <see cref="IPv4Command"/>.
    /// </summary>
    public IPv4Command() : base(
        CommandLineStrings_NetworkInterfaces.IPv4CommandName,
        CommandLineStrings_NetworkInterfaces.IPv4CommandDescription)
    {
        Add(new DHCPCommand());
        Add(new IPCommand());
        Add(new GatewayCommand());
        Add(new VLANCommand());
    }
}
