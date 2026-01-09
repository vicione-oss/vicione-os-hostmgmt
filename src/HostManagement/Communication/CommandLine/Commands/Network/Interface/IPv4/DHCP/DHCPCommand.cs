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
    public DHCPCommand() : base(
        CommandLineStrings_NetworkInterfaces.DHCPCommandName,
        CommandLineStrings_NetworkInterfaces.DHCPCommandDescription)
    {
        Add(new DHCPEnableCommand(CommonOptions.s_interfaceNameOption));
        Add(new DHCPDisableCommand(CommonOptions.s_interfaceNameOption));
        Add(new DHCPListCommand());
        Add(new DHCPRenewLeaseCommand(CommonOptions.s_interfaceNameOption));
    }
}
