using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;

/// <summary>
/// Represents the DHCP disable command.
/// </summary>
internal sealed class DHCPDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DHCPDisableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    public DHCPDisableCommand(Option<string> interfaceNameOption) : base(
        CommandLineStrings_Common.DisableCommandName,
        CommandLineStrings_NetworkInterfaces.DHCPDisableCommandDescription)
    {
        Add(interfaceNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return DisableHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                cancellationToken);
        });
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

        if (!interfaceToConfigure.IPv4.DHCPEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.DHCPAlreadyDisabled, interfaceName);
            return;
        }

        interfaceToConfigure.IPv4.DHCPEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.DHCPDisabled, interfaceName);
    }
}
