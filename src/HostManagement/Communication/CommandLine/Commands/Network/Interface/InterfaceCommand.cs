using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;
using HostManagement.Communication.CommandLine.Resources;

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
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public InterfaceCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Network.Interface.CommandName, CliStrings.Network.Interface.CommandDescription)
    {
        Add(new InterfaceStatusCommand(interfaceNameArgument));
        Add(new InterfaceEnableCommand(interfaceNameArgument));
        Add(new InterfaceDisableCommand(interfaceNameArgument));
        Add(new InterfaceResetCommand(interfaceNameArgument));
        Add(new InterfaceMACCommand(CommonOptions.s_interfaceNameOption, s_macAddressOption));
        Add(new VLANCommand(interfaceNameArgument));
        Add(new IPv4Command(interfaceNameArgument));
    }
}
