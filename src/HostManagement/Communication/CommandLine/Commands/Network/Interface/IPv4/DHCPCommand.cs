using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

/// <summary>
/// Represents the DHCP command.
/// </summary>
internal sealed class DHCPCommand : ConfigCommandBase
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;

    /// <summary>
    /// Initializes a new <see cref="DHCPCommand"/>.
    /// </summary>
    public DHCPCommand() : base(
        CommandLineStrings_NetworkInterfaces.DHCPCommandName,
        CommandLineStrings_NetworkInterfaces.DHCPCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_NetworkInterfaces.DHCPEnableCommandDescription,
            s_nameOption,
            (name) => EnableDisableHandler(interfaceName: name, enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_NetworkInterfaces.DHCPDisableCommandDescription,
            s_nameOption,
            (name) => EnableDisableHandler(interfaceName: name, enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_NetworkInterfaces.DHCPListCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_NetworkInterfaces.DHCPRenewLeaseCommandName,
            CommandLineStrings_NetworkInterfaces.DHCPRenewLeaseCommandDescription,
            s_nameOption,
            RenewLeaseHandler);
    }

    private static async Task EnableDisableHandler(string interfaceName, bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.DHCPEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_NetworkInterfaces.DHCPAlreadyEnabled
                : CommandLineStrings_NetworkInterfaces.DHCPAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info, interfaceName);

            return;
        }

        interfaceToConfigure.IPv4.DHCPEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_NetworkInterfaces.DHCPEnabled
            : CommandLineStrings_NetworkInterfaces.DHCPDisabled;

        SimpleOutput.WriteLine(enableMessage, interfaceName);
    }

    private static async Task RenewLeaseHandler(string interfaceName)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new KeyValueOutput();

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            output
                .WithWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName)
                .WriteLine();

            return;
        }

        if (!interfaceToConfigure.IPv4.DHCPEnabled)
        {
            output
                .WithInformation(CommandLineStrings_NetworkInterfaces.DHCPIsDisabled, interfaceName)
                .WriteLine();

            return;
        }

        if (await NamedPipeRequestHelper.RenewDHCPLease(interfaceName) is not DHCPLease dhcpLease)
            return;

        output
            .WithTitle(CommandLineStrings_NetworkInterfaces.DHCPLeaseRenewed)
            .With(new(CommandLineStrings_NetworkInterfaces.DHCPLeaseObtained, dhcpLease.LeaseObtained))
            .With(new(CommandLineStrings_NetworkInterfaces.DHCPLeaseExpires, dhcpLease.LeaseExpires))
            .With(new(CommandLineStrings_NetworkInterfaces.Gateway, dhcpLease.Gateway))
            .With(new(CommandLineStrings_NetworkInterfaces.IPAddress, dhcpLease.IPv4Detail?.IPAddress))
            .With(new(CommandLineStrings_NetworkInterfaces.SubnetMask, dhcpLease.IPv4Detail?.Netmask))
            .WriteLine();
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new PropertyListOutput().WithTitle(CommandLineStrings_NetworkInterfaces.Interfaces);

        foreach (var @interface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var dhcpEnabledString = @interface.IPv4.DHCPEnabled
                ? CommandLineStrings_Common.enabled
                : CommandLineStrings_Common.disabled;

            output.With(
                @interface.CommonInformation.Name,
                new(CommandLineStrings_NetworkInterfaces.DHCP, dhcpEnabledString));
        }

        output.WriteLine();
    }
}
