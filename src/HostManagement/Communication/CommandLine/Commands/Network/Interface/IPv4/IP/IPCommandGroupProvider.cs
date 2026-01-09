using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.IP;

/// <summary>
/// Represents the <see cref="IPCommandGroupProvider"/>.
/// </summary>
internal static class IPCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;
    private static readonly Option<IPAddress> s_oldAddressOption = CommonOptions.s_oldIpAddressOption;
    private static readonly Option<IPAddress> s_addressOption = CommonOptions.s_ipAddressOption;
    private static readonly Option<IPAddress> s_maskOption = CommonOptions.s_netmaskOption;

    private static readonly Command s_addCommand = CommandHelper.ConstructCommand(
        "add",
        "Add a new IPv4 address and netmask to the specified network interface.",
        s_nameOption,
        s_addressOption,
        s_maskOption,
        AddHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove the IPv4 address of the specified network interface.",
        s_nameOption,
        s_addressOption,
        RemoveHandler);

    private static readonly Command s_modifyCommand = CommandHelper.ConstructCommand(
        "modify",
        "Modify the IPv4 address of the specified network interface.",
        s_nameOption,
        s_oldAddressOption,
        s_addressOption,
        s_maskOption,
        ModifyHandler);

    /// <summary>
    /// Adds the command group to configure IP settings of a specified network interface.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddIPCommandGroup(this Command parentCommand)
    {
        var ipCommand = parentCommand.AddCommand(
            "ip",
            "Configure IPv4 address and netmask of a specified network interface.");

        ipCommand.AddCommand(s_addCommand);
        ipCommand.AddCommand(s_removeCommand);
        ipCommand.AddCommand(s_modifyCommand);
    }

    private static async Task AddHandler(
        string interfaceName,
        IPAddress addressOptionValue,
        IPAddress maskOptionValue)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        var showDhcpDisabledMessage = false;

        if (interfaceToConfigure.IPv4.IPv4Details.Count == 0)
        {
            interfaceToConfigure.IPv4.DHCPEnabled = false;
            showDhcpDisabledMessage = true;
        }

        interfaceToConfigure.IPv4.IPv4Details.Add(
            new IPv4Detail()
            {
                IPAddress = addressOptionValue,
                Netmask = maskOptionValue
            });

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (showDhcpDisabledMessage)
            SimpleOutput.WriteLineWarning(CommandLineStrings.DHCPDisabled, interfaceName);

        SimpleOutput.WriteLine(
            CommandLineStrings.NetworkInterfaceIPAdded,
            addressOptionValue,
            interfaceName);
    }

    private static async Task RemoveHandler(
        string interfaceName,
        IPAddress ipAddress)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        var ipv4DetailToRemove = interfaceToConfigure.IPv4.IPv4Details
            .FirstOrDefault(ipv4Detail => ipv4Detail.IPAddress.Equals(ipAddress));

        if (ipv4DetailToRemove is null)
        {
            SimpleOutput.WriteLineWarning(
                CommandLineStrings.NetworkInterfaceIPNotFound,
                ipAddress,
                interfaceName);

            return;
        }

        interfaceToConfigure.IPv4.IPv4Details.Remove(ipv4DetailToRemove);

        var showDhcpEnabledMessage = false;

        if (interfaceToConfigure.IPv4.IPv4Details.Count == 0)
        {
            interfaceToConfigure.IPv4.DHCPEnabled = true;
            showDhcpEnabledMessage = true;
        }

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (showDhcpEnabledMessage)
            SimpleOutput.WriteLineWarning(CommandLineStrings.DHCPEnabled, interfaceName);

        SimpleOutput.WriteLine(
            CommandLineStrings.NetworkInterfaceIPRemoved,
            ipAddress,
            interfaceName);
    }

    private static async Task ModifyHandler(
        string interfaceName,
        IPAddress oldAddressOptionValue,
        IPAddress newAddressOptionValue,
        IPAddress maskOptionValue)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        foreach (var ipv4Detail in interfaceToConfigure.IPv4.IPv4Details)
        {
            if (ipv4Detail.IPAddress.Equals(newAddressOptionValue))
            {
                SimpleOutput.WriteLineWarning(
                    CommandLineStrings.NetworkInterfaceIPAlreadyDefined,
                    newAddressOptionValue);

                return;
            }

            if (ipv4Detail.IPAddress.Equals(oldAddressOptionValue))
            {
                ipv4Detail.IPAddress = newAddressOptionValue;
                ipv4Detail.Netmask = maskOptionValue;

                if (!await NamedPipeRequestHelper.SetConfiguration(config))
                    return;

                if (interfaceToConfigure.IPv4.DHCPEnabled)
                    SimpleOutput.WriteLineWarning(CommandLineStrings.DHCPIsEnabled, interfaceName);

                SimpleOutput.WriteLine(
                    CommandLineStrings.NetworkInterfaceIPModified,
                    interfaceName,
                    oldAddressOptionValue,
                    newAddressOptionValue);

                return;
            }
        }

        SimpleOutput.WriteLineWarning(
            CommandLineStrings.NetworkInterfaceIPNotFound,
            oldAddressOptionValue,
            interfaceName);
    }
}
