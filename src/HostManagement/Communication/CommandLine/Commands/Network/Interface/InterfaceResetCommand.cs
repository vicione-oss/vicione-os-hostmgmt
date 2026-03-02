using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Extensions;

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
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is null)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        if (!await NamedPipeRequestHelper.ResetNetworkInterface(interfaceName, cancellationToken))
        {
            SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.InterfaceResetFailed, interfaceName);
            return;
        }

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.InterfaceResetSucceeded, interfaceName);
    }
}
