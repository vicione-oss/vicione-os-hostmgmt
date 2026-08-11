using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.Gateway;

/// <summary>
/// Represents the gateway set command.
/// </summary>
internal sealed class GatewaySetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="GatewaySetCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    /// <param name="addressOption">The IP address option.</param>
    public GatewaySetCommand(Option<string> interfaceNameOption, Option<IPAddress> addressOption) : base(
        CommandLineStrings_Common.SetCommandName,
        CommandLineStrings_NetworkInterfaces.GatewaySetCommandDescription)
    {
        Add(interfaceNameOption);
        Add(addressOption);

        SetAction((parseResult, cancellationToken)
            => SetHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                parseResult.GetRequiredValue(addressOption),
                cancellationToken));
    }

    private static async Task SetHandler(string interfaceName, IPAddress address, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        if (interfaceToConfigure.IPv4.IPv4Details.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_NetworkInterfaces.InterfaceIPsNotSet, interfaceName);
            return;
        }

        if (interfaceToConfigure.IPv4.Gateway is not null)
        {
            if (interfaceToConfigure.IPv4.Gateway.Equals(address))
            {
                SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.GatewayAlreadyDefined, address);
                return;
            }

            var oldAddress = interfaceToConfigure.IPv4.Gateway;
            interfaceToConfigure.IPv4.Gateway = address;

            if (!await SetConfiguration(config, cancellationToken))
                return;

            if (interfaceToConfigure.IPv4.DHCPEnabled)
                SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);

            SimpleOutput.WriteLine(
                CommandLineStrings_NetworkInterfaces.GatewaySet,
                oldAddress,
                address,
                interfaceToConfigure.CommonInformation.Name);
        }
        else
        {
            interfaceToConfigure.IPv4.Gateway = address;

            if (!await SetConfiguration(config, cancellationToken))
                return;

            if (interfaceToConfigure.IPv4.DHCPEnabled)
                SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);

            SimpleOutput.WriteLine(
                CommandLineStrings_NetworkInterfaces.GatewayAdded,
                address,
                interfaceToConfigure.CommonInformation.Name);
        }
    }
}
