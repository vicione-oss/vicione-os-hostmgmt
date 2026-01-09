using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;

/// <summary>
/// Represents the DHCP enable command.
/// </summary>
internal sealed class DHCPEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DHCPEnableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    public DHCPEnableCommand(Option<string> interfaceNameOption) : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_NetworkInterfaces.DHCPEnableCommandDescription)
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

        if (interfaceToConfigure.IPv4.DHCPEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.DHCPAlreadyEnabled, interfaceName);
            return;
        }

        interfaceToConfigure.IPv4.DHCPEnabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.DHCPEnabled, interfaceName);
    }
}
