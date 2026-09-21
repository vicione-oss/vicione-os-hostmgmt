using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.MAC;

/// <summary>
/// Represents the mac command.
/// </summary>
internal sealed class MACCommand : ConfigCommandBase
{
    internal static readonly Option<string> s_macAddressOption = new("--address", "-a")
    {
        Description = "The MAC address of the specified network interface.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="MACCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public MACCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Network.Interface.MAC.CommandName, CliStrings.Network.Interface.MAC.CommandDescription)
    {
        Add(new MACStatusCommand(interfaceNameArgument));
        Add(new MACEnableCommand(interfaceNameArgument));
        Add(new MACDisableCommand(interfaceNameArgument));
        Add(new MACSetCommand(CommonOptions.s_interfaceNameOption, s_macAddressOption));
        Add(new MACResetCommand(interfaceNameArgument));
    }
}
