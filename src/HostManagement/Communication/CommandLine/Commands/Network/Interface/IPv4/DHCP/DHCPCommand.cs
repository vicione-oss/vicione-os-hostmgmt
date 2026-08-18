using System.CommandLine;

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
    public DHCPCommand(Argument<string> interfaceNameArgument) : base(
        CommandLineStrings_NetworkInterfaces.DHCPCommandName,
        CommandLineStrings_NetworkInterfaces.DHCPCommandDescription)
    {
        Add(new DHCPListCommand());
        Add(new DHCPEnableCommand(interfaceNameArgument));
        Add(new DHCPDisableCommand(interfaceNameArgument));
        Add(new DHCPRenewLeaseCommand(interfaceNameArgument));
    }
}
