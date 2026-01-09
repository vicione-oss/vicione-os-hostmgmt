using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.VLAN;

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
    public VLANCommand() : base(
        CommandLineStrings_NetworkInterfaces.VLANCommandName,
        CommandLineStrings_NetworkInterfaces.VLANCommandDescription)
    {
        Add(new VLANEnableCommand(CommonOptions.s_interfaceNameOption));
        Add(new VLANDisableCommand(CommonOptions.s_interfaceNameOption));
        Add(new VLANListCommand());
        Add(new VLANSetCommand(CommonOptions.s_interfaceNameOption, s_vlanIdOption));
        Add(new VLANRemoveCommand(CommonOptions.s_interfaceNameOption));
    }
}
