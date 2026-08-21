using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.Gateway;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

/// <summary>
/// Represents the IPv4 command.
/// </summary>
internal sealed class IPv4Command : Command
{
    /// <summary>
    /// Initializes a new <see cref="IPv4Command"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public IPv4Command(Argument<string> interfaceNameArgument)
        : base(CliStrings.Network.Interface.IPv4.CommandName, CliStrings.Network.Interface.IPv4.CommandDescription)
    {
        Add(new DHCPCommand(interfaceNameArgument));
        Add(new IPCommand());
        Add(new GatewayCommand());
    }
}
