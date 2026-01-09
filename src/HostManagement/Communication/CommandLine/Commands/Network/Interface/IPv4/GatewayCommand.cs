using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

/// <summary>
/// Represents the gateway command.
/// </summary>
internal sealed class GatewayCommand : ConfigCommandBase
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;
    private static readonly Option<IPAddress> s_addressOption = CommonOptions.s_ipAddressOption;

    /// <summary>
    /// Initializes a new <see cref="GatewayCommand"/>.
    /// </summary>
    public GatewayCommand() : base(
        CommandLineStrings_NetworkInterfaces.GatewayCommandName,
        CommandLineStrings_NetworkInterfaces.GatewayCommandDescription)
    {
        Add(
            CommandLineStrings_NetworkInterfaces.GatewaySetCommandName,
            CommandLineStrings_NetworkInterfaces.GatewaySetCommandDescription,
            s_nameOption,
            s_addressOption,
            SetHandler);

        Add(CommandLineStrings_NetworkInterfaces.GatewayRemoveCommandName,
            CommandLineStrings_NetworkInterfaces.GatewayRemoveCommandDescription,
            RemoveHandler);
    }

    private static async Task SetHandler(string interfaceName, IPAddress address)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
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

            if (!await SetConfiguration(config))
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

            if (!await SetConfiguration(config))
                return;

            if (interfaceToConfigure.IPv4.DHCPEnabled)
                SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);

            SimpleOutput.WriteLine(
                CommandLineStrings_NetworkInterfaces.GatewayAdded,
                address,
                interfaceToConfigure.CommonInformation.Name);
        }
    }

    private static async Task RemoveHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var interfaceToConfigure = config.NetworkInterfacesSettings.NetworkInterfaces
            .FirstOrDefault(interfaceDetail => interfaceDetail.IPv4.Gateway is not null);

        if (interfaceToConfigure is null)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.GatewayIsNotConfigured);

            return;
        }

        var oldAddress = interfaceToConfigure.IPv4.Gateway;
        interfaceToConfigure.IPv4.Gateway = null;

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.GatewayRemoved, oldAddress);
    }
}
