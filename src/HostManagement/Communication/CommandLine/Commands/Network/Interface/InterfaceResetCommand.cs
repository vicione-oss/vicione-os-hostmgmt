using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface reset command.
/// </summary>
internal sealed class InterfaceResetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="InterfaceResetCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    public InterfaceResetCommand(Option<string> interfaceNameOption) : base(
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceResetCommandName,
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceResetCommandDescription)
    {
        Add(interfaceNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return ResetHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                cancellationToken);
        });
    }

    private static async Task ResetHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.ResetNetworkInterface(interfaceName, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.InterfaceResetSucceeded, interfaceName);
    }
}
