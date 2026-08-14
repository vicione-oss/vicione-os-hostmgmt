using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;

/// <summary>
/// Represents the IP command.
/// </summary>
internal sealed class IPCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="IPCommand"/>.
    /// </summary>
    public IPCommand()
        : base(CliStrings.Network.Interface.IP.CommandName, CliStrings.Network.Interface.IP.CommandDescription)
    {
        Add(new IPAddCommand(
            CommonOptions.s_interfaceNameOption,
            CommonOptions.s_ipAddressOption,
            CommonOptions.s_netmaskOption));

        Add(new IPRemoveCommand(
            CommonOptions.s_interfaceNameOption,
            CommonOptions.s_ipAddressOption));

        Add(new IPModifyCommand(
            CommonOptions.s_interfaceNameOption,
            CommonOptions.s_oldIpAddressOption,
            CommonOptions.s_ipAddressOption,
            CommonOptions.s_netmaskOption));
    }
}
