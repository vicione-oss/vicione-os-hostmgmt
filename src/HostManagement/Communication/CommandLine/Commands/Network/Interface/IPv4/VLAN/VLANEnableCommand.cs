using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.VLAN;

/// <summary>
/// Represents the VLAN enable command.
/// </summary>
internal sealed class VLANEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="VLANEnableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    public VLANEnableCommand(Option<string> interfaceNameOption) : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_NetworkInterfaces.VLANEnableCommandDescription)
    {
        Add(interfaceNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return EnableHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                cancellationToken);
        });
    }

    private static async Task EnableHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        if (interfaceToConfigure.IPv4.VLANEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.VLANAlreadyEnabled, interfaceName);
            return;
        }

        interfaceToConfigure.IPv4.VLANEnabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.VLANEnabled, interfaceName);
    }
}
