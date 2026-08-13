using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface disable command.
/// </summary>
internal sealed class InterfaceDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="InterfaceDisableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public InterfaceDisableCommand(Argument<string> interfaceNameArgument) : base(
        CliStrings.Common.DisableCommandName,
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceDisableCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => DisableHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task DisableHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        if (!interfaceToConfigure.CommonInformation.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.InterfaceAlreadyDisabled, interfaceName);
            return;
        }

        interfaceToConfigure.CommonInformation.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.InterfaceDisabled, interfaceName);
    }
}
