using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

/// <summary>
/// Represents the IP command.
/// </summary>
internal sealed class IPCommand : ConfigCommandBase
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;
    private static readonly Option<IPAddress> s_oldAddressOption = CommonOptions.s_oldIpAddressOption;
    private static readonly Option<IPAddress> s_addressOption = CommonOptions.s_ipAddressOption;
    private static readonly Option<IPAddress> s_maskOption = CommonOptions.s_netmaskOption;

    /// <summary>
    /// Initializes a new <see cref="IPCommand"/>.
    /// </summary>
    public IPCommand() : base(
        CommandLineStrings_NetworkInterfaces.IPCommandName,
        CommandLineStrings_NetworkInterfaces.IPCommandDescription)
    {
        Add(
            CommandLineStrings_NetworkInterfaces.IPAddCommandName,
            CommandLineStrings_NetworkInterfaces.IPAddCommandDescription,
            s_nameOption,
            s_addressOption,
            s_maskOption,
            AddHandler);

        Add(
            CommandLineStrings_NetworkInterfaces.IPRemoveCommandName,
            CommandLineStrings_NetworkInterfaces.IPRemoveCommandDescription,
            s_nameOption,
            s_addressOption,
            RemoveHandler);

        Add(
            CommandLineStrings_NetworkInterfaces.IPModifyCommandName,
            CommandLineStrings_NetworkInterfaces.IPModifyCommandDescription,
            s_nameOption,
            s_oldAddressOption,
            s_addressOption,
            s_maskOption,
            ModifyHandler);
    }

    private static async Task AddHandler(string interfaceName, IPAddress ipAddress, IPAddress netmask)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        interfaceToConfigure.IPv4.IPv4Details.Add(new IPv4Detail()
        {
            IPAddress = ipAddress,
            Netmask = netmask
        });

        if (!await SetConfiguration(config))
            return;

        if (interfaceToConfigure.IPv4.DHCPEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);

        SimpleOutput.WriteLine(
            CommandLineStrings_NetworkInterfaces.InterfaceIPAdded,
            ipAddress,
            netmask,
            interfaceName);
    }

    private static async Task RemoveHandler(string interfaceName, IPAddress ipAddress)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        var ipv4DetailToRemove = interfaceToConfigure.IPv4.IPv4Details
            .FirstOrDefault(ipv4Detail => ipv4Detail.IPAddress.Equals(ipAddress));

        if (ipv4DetailToRemove is null)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceIPNotFound, ipAddress, interfaceName);
            return;
        }

        interfaceToConfigure.IPv4.IPv4Details.Remove(ipv4DetailToRemove);

        var output = new ListOutput();

        if (interfaceToConfigure.IPv4.IPv4Details.Count == 0)
        {
            if (interfaceToConfigure.IPv4.Gateway is not null)
            {
                var removedGateway = interfaceToConfigure.IPv4.Gateway;
                interfaceToConfigure.IPv4.Gateway = null;

                output.WithInformation(CommandLineStrings_NetworkInterfaces.GatewayRemoved, removedGateway);
            }

            if (!interfaceToConfigure.IPv4.DHCPEnabled)
            {
                interfaceToConfigure.IPv4.DHCPEnabled = true;
                output.WithInformation(CommandLineStrings_NetworkInterfaces.DHCPEnabled, interfaceName);
            }
        }
        else if (interfaceToConfigure.IPv4.DHCPEnabled)
        {
            output.WithWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);
        }

        if (!await SetConfiguration(config))
            return;

        if (output.HasMessages)
            output.WriteLine();

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.InterfaceIPRemoved, ipAddress, interfaceName);
    }

    private static async Task ModifyHandler(
        string interfaceName,
        IPAddress oldAddress,
        IPAddress newAddress,
        IPAddress newNetmask)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        foreach (var ipv4Detail in interfaceToConfigure.IPv4.IPv4Details)
        {
            if (ipv4Detail.IPAddress.Equals(newAddress) &&
                ipv4Detail.Netmask.Equals(newNetmask))
            {
                SimpleOutput.WriteLineInformation(
                    CommandLineStrings_NetworkInterfaces.InterfaceIPAlreadyDefined,
                    newAddress,
                    newNetmask);

                return;
            }

            if (ipv4Detail.IPAddress.Equals(oldAddress))
            {
                var oldNetmask = ipv4Detail.Netmask;

                ipv4Detail.IPAddress = newAddress;
                ipv4Detail.Netmask = newNetmask;

                if (!await SetConfiguration(config))
                    return;

                if (interfaceToConfigure.IPv4.DHCPEnabled)
                    SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.DHCPIsEnabled, interfaceName);

                SimpleOutput.WriteLine(
                    CommandLineStrings_NetworkInterfaces.InterfaceIPModified,
                    interfaceName,
                    oldAddress,
                    oldNetmask,
                    newAddress,
                    newNetmask);

                return;
            }
        }

        SimpleOutput.WriteLineWarning(
            CommandLineStrings_NetworkInterfaces.InterfaceIPNotFound,
            oldAddress,
            interfaceName);
    }
}
