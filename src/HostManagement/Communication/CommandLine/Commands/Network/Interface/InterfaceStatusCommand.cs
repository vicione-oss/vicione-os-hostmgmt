using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public InterfaceStatusCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.StatusCommandName, CliStrings.Network.Interface.StatusCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => StatusHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task StatusHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        var enabledStateMessage = interfaceToConfigure.CommonInformation.Enabled
            ? CliStrings.Network.Interface.IsEnabled
            : CliStrings.Network.Interface.IsDisabled;

        SimpleOutput.WriteLine(enabledStateMessage, interfaceName);
    }
}
