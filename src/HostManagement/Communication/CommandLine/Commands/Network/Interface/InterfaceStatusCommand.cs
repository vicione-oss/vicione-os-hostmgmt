using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface status command.
/// </summary>
internal sealed class InterfaceStatusCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="InterfaceStatusCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    public InterfaceStatusCommand(Option<string> interfaceNameOption) : base(
        CommandLineStrings_Common.StatusCommandName,
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceStatusCommandDescription)
    {
        Add(interfaceNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return StatusHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                cancellationToken);
        });
    }

    private static async Task StatusHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        var enabledStateMessage = interfaceToConfigure.CommonInformation.Enabled
            ? CommandLineStrings_NetworkInterfaces.InterfaceIsEnabled
            : CommandLineStrings_NetworkInterfaces.InterfaceIsDisabled;

        SimpleOutput.WriteLine(enabledStateMessage, interfaceName);
    }
}
