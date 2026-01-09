using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

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
        string nameOptionValue,
        IPAddress addressOptionValue,
        IPAddress maskOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var networkInterfaces = config.NetworkInterfacesSettings.NetworkInterfaces;

        foreach (var networkInterface in networkInterfaces)
        {
            if (!networkInterface.CommonInformation.Name.Equals(nameOptionValue))
                continue;

            var showDhcpDisabledMessage = false;

            if (networkInterface.IPv4.IPv4Details.Count == 0)
            {
                networkInterface.IPv4.DHCPEnabled = false;
                showDhcpDisabledMessage = true;
            }

            networkInterface.IPv4.IPv4Details.Add(
                new IPv4Detail()
                {
                    IPAddress = addressOptionValue,
                    Netmask = maskOptionValue
                });

            if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
                return;

            if (showDhcpDisabledMessage)
            {
                ConsoleEx.WriteLineWarning(
                    CommandLineStrings.DHCPDisabled,
                    nameOptionValue);
            }

            Console.WriteLine(
                CommandLineStrings.NetworkInterfaceIPAdded,
                addressOptionValue,
                nameOptionValue);

            return;
        }

        ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, nameOptionValue);
    }

    private static async Task RemoveHandler(
        string nameOptionValue,
        IPAddress addressOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var networkInterfaces = config.NetworkInterfacesSettings.NetworkInterfaces;

        foreach (var networkInterface in networkInterfaces)
        {
            if (!networkInterface.CommonInformation.Name.Equals(nameOptionValue))
                continue;

            foreach (var ipv4Detail in networkInterface.IPv4.IPv4Details)
            {
                if (!ipv4Detail.IPAddress.Equals(addressOptionValue))
                    continue;

                networkInterface.IPv4.IPv4Details.Remove(ipv4Detail);

                var showDhcpEnabledMessage = false;

                if (networkInterface.IPv4.IPv4Details.Count == 0)
                {
                    networkInterface.IPv4.DHCPEnabled = true;
                    showDhcpEnabledMessage = true;
                }

                if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
                    return;

                if (showDhcpEnabledMessage)
                {
                    ConsoleEx.WriteLineWarning(
                        CommandLineStrings.DHCPEnabled,
                        nameOptionValue);
                }

                Console.WriteLine(
                    CommandLineStrings.NetworkInterfaceIPRemoved,
                    addressOptionValue,
                    nameOptionValue);

                return;
            }

            ConsoleEx.WriteLineError(
                CommandLineStrings.NetworkInterfaceIPNotFound,
                addressOptionValue,
                nameOptionValue);

            return;
        }

        ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, nameOptionValue);
    }

    private static async Task ModifyHandler(
        string nameOptionValue,
        IPAddress oldAddressOptionValue,
        IPAddress newAddressOptionValue,
        IPAddress maskOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var networkInterfaces = config.NetworkInterfacesSettings.NetworkInterfaces;

        foreach (var networkInterface in networkInterfaces)
        {
            if (networkInterface.CommonInformation.Name.Equals(nameOptionValue))
            {
                foreach (var ipv4Detail in networkInterface.IPv4.IPv4Details)
                {
                    if (ipv4Detail.IPAddress.Equals(newAddressOptionValue))
                    {
                        ConsoleEx.WriteLineWarning(CommandLineStrings.NetworkInterfaceIPAlreadyDefined, newAddressOptionValue);
                        return;
                    }

                    if (ipv4Detail.IPAddress.Equals(oldAddressOptionValue))
                    {
                        ipv4Detail.IPAddress = newAddressOptionValue;
                        ipv4Detail.Netmask = maskOptionValue;

                        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
                            return;

                        Console.WriteLine(
                            CommandLineStrings.NetworkInterfaceIPModified,
                            nameOptionValue,
                            oldAddressOptionValue,
                            newAddressOptionValue);

                        return;
                    }
                }

                ConsoleEx.WriteLineError(
                    CommandLineStrings.NetworkInterfaceIPNotFound,
                    oldAddressOptionValue,
                    nameOptionValue);

                return;
            }
        }

        ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, nameOptionValue);
    }
}
