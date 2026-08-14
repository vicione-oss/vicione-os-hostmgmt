using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;

/// <summary>
/// Represents the VLAN command.
/// </summary>
internal sealed class VLANCommand : Command
{
    private static readonly Option<int> s_vlanIdOption = new("--vlan-id", "-i")
    {
        Description = "The VLAN ID.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="VLANCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public VLANCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Network.Interface.VLAN.CommandName, CliStrings.Network.Interface.VLAN.CommandDescription)
    {
        Add(new VLANListCommand());
        Add(new VLANEnableCommand(interfaceNameArgument));
        Add(new VLANDisableCommand(interfaceNameArgument));
        Add(new VLANRemoveCommand(interfaceNameArgument));
        Add(new VLANSetCommand(CommonOptions.s_interfaceNameOption, s_vlanIdOption));
    }
}
