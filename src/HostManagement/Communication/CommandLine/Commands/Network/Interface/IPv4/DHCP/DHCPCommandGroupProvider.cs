using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.DHCP;

/// <summary>
/// Represents the <see cref="DHCPCommandGroupProvider"/>.
/// </summary>
internal static class DHCPCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable DHCP of the specified network interface.",
        s_nameOption,
        EnableHandler);

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable DHCP of the specified network interface.",
        s_nameOption,
        DisableHandler);

    private static readonly Command s_renewLeaseCommand = CommandHelper.ConstructCommand(
        "renew-lease",
        "Renew DHCP lease of the specified network interface.",
        s_nameOption,
        RenewLeaseHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List the DHCP enabled state of all available network interfaces.",
        ListHandler);

    /// <summary>
    /// Adds the command group to configure DHCP of a specified network interface.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddDHCPCommandGroup(this Command parentCommand)
    {
        var dhcpCommand = parentCommand.AddCommand(
            "dhcp",
            "Configure DHCP of a specified network interface.");

        dhcpCommand.AddCommand(s_enableCommand);
        dhcpCommand.AddCommand(s_disableCommand);
        dhcpCommand.AddCommand(s_renewLeaseCommand);
        dhcpCommand.AddCommand(s_listCommand);
    }

    private static async Task EnableHandler(string name)
        => await EnableDisableHandler(name, enable: true);

    private static async Task DisableHandler(string name)
        => await EnableDisableHandler(name, enable: false);

    private static async Task EnableDisableHandler(string interfaceName, bool enable)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.DHCPEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.DHCPAlreadyEnabled
                : CommandLineStrings.DHCPAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info, interfaceName);

            return;
        }

        interfaceToConfigure.IPv4.DHCPEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings.DHCPEnabled
            : CommandLineStrings.DHCPDisabled;

        SimpleOutput.WriteLine(enableMessage, interfaceName);
    }

    private static async Task RenewLeaseHandler(string interfaceName)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new KeyValueOutput();

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            output
                .WithWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName)
                .WriteLine();

            return;
        }

        if (!interfaceToConfigure.IPv4.DHCPEnabled)
        {
            output
                .WithInformation(CommandLineStrings.DHCPIsDisabled, interfaceName)
                .WriteLine();

            return;
        }

        if (await NamedPipeRequestHelper.RenewDHCPLease(interfaceName) is not DHCPLease dhcpLease)
            return;

        output
            .WithTitle(CommandLineStrings.DHCPLeaseRenewed)
            .With(new(CommandLineStrings.DHCPLeaseObtained, dhcpLease.LeaseObtained))
            .With(new(CommandLineStrings.DHCPLeaseExpires, dhcpLease.LeaseExpires))
            .With(new(CommandLineStrings.Gateway, dhcpLease.Gateway))
            .With(new(CommandLineStrings.IPAddress, dhcpLease.IPv4Detail?.IPAddress))
            .With(new(CommandLineStrings.SubnetMask, dhcpLease.IPv4Detail?.Netmask))
            .WriteLine();
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new PropertyListOutput().WithTitle(CommandLineStrings.Interfaces);

        foreach (var @interface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var dhcpEnabledString = @interface.IPv4.DHCPEnabled
                ? CommandLineStrings.enabledLower
                : CommandLineStrings.disabled;

            output.With(
                @interface.CommonInformation.Name,
                new(CommandLineStrings.DHCP, dhcpEnabledString));
        }

        output.WriteLine();
    }
}
