using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.Gateway;

/// <summary>
/// Represents the <see cref="GatewayCommandGroupProvider"/>.
/// </summary>
internal static class GatewayCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;
    private static readonly Option<IPAddress> s_addressOption = CommonOptions.s_ipAddressOption;

    private static readonly Command s_setCommand = CommandHelper.ConstructCommand(
        "set",
        "Remove the existing default gateway and set a new one. Only one gateway can be specified.",
        s_nameOption,
        s_addressOption,
        SetHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove the default gateway.",
        RemoveHandler);

    /// <summary>
    /// Adds the command group to configure gateway settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddGatewayCommandGroup(this Command parentCommand)
    {
        var gatewayCommand = parentCommand.AddCommand(
            "gateway",
            "Configure the default gateway.");

        gatewayCommand.AddCommand(s_setCommand);
        gatewayCommand.AddCommand(s_removeCommand);
    }

    private static async Task SetHandler(string interfaceName, IPAddress address)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.Gateway is not null)
        {
            if (interfaceToConfigure.IPv4.Gateway.Equals(address))
            {
                SimpleOutput.WriteLineInformation(CommandLineStrings.GatewayAlreadyDefined, address);

                return;
            }

            var oldAddress = interfaceToConfigure.IPv4.Gateway;
            interfaceToConfigure.IPv4.Gateway = address;

            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            SimpleOutput.WriteLine(
                CommandLineStrings.GatewaySet,
                oldAddress,
                address,
                interfaceToConfigure.CommonInformation.Name);
        }
        else
        {
            interfaceToConfigure.IPv4.Gateway = address;

            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            SimpleOutput.WriteLine(
                CommandLineStrings.GatewayAdded,
                address,
                interfaceToConfigure.CommonInformation.Name);
        }
    }

    private static async Task RemoveHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var interfaceToConfigure = config.NetworkInterfacesSettings.NetworkInterfaces
            .FirstOrDefault(interfaceDetail => interfaceDetail.IPv4.Gateway is not null);

        if (interfaceToConfigure is null)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.GatewayIsNotConfigured);

            return;
        }

        var oldAddress = interfaceToConfigure.IPv4.Gateway;
        interfaceToConfigure.IPv4.Gateway = null;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.GatewayRemoved, oldAddress);
    }
}
