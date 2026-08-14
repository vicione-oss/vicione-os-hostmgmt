using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;

/// <summary>
/// Represents the DHCP command.
/// </summary>
internal sealed class DHCPCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="DHCPCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public DHCPCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Network.Interface.DHCP.CommandName, CliStrings.Network.Interface.DHCP.CommandDescription)
    {
        Add(new DHCPListCommand());
        Add(new DHCPEnableCommand(interfaceNameArgument));
        Add(new DHCPDisableCommand(interfaceNameArgument));
        Add(new DHCPRenewLeaseCommand(interfaceNameArgument));
    }
}
