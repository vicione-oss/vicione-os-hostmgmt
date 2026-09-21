using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface reset command.
/// </summary>
internal sealed class InterfaceResetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="InterfaceResetCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public InterfaceResetCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.ResetCommandName, CliStrings.Network.Interface.ResetCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => ResetHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task ResetHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.ResetNetworkInterface(interfaceName, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Interface.ResetSucceeded, interfaceName);
    }
}
