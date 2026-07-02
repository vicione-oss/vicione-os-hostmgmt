using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface command.
/// </summary>
internal sealed class InterfaceCommand : Command
{
    internal static readonly Option<string> s_macAddressOption = new("--address", "-a")
    {
        Description = "The MAC address of the specified network interface.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="InterfaceCommand"/>.
    /// </summary>
    public InterfaceCommand() : base(
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceCommandName,
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceCommandDescription)
    {
        Add(new InterfaceEnableCommand(CommonOptions.s_interfaceNameOption));
        Add(new InterfaceDisableCommand(CommonOptions.s_interfaceNameOption));
        Add(new InterfaceStatusCommand(CommonOptions.s_interfaceNameOption));
        Add(new InterfaceResetCommand(CommonOptions.s_interfaceNameOption));
        Add(new InterfaceMACCommand(CommonOptions.s_interfaceNameOption, s_macAddressOption));
        Add(new VLANCommand());
        Add(new IPv4Command());
    }
}
