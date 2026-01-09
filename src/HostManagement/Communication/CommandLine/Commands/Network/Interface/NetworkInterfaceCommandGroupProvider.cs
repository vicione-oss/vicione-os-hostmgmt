using System.CommandLine;
using HostManagement.Common.Utilities;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the <see cref="NetworkInterfaceCommandGroupProvider"/>.
/// </summary>
internal static class NetworkInterfaceCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;

    internal static readonly Option<string> s_macAddressOption = new(["-a", "--address"])
    {
        Description = "The MAC address of the specified network interface.",
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable the specified network interface.",
        s_nameOption,
        EnableHandler);

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable the specified network interface.",
        s_nameOption,
        DisableHandler);

    private static readonly Command s_statusCommand = CommandHelper.ConstructCommand(
        "status",
        "Show enabled status of the specified network interface.",
        s_nameOption,
        StatusHandler);

    private static readonly Command s_setMACCommand = CommandHelper.ConstructCommand(
        "mac",
        "Get or set the MAC address of the specified network interface.",
        s_nameOption,
        s_macAddressOption,
        MACAddressHandler);

    /// <summary>
    /// Adds the command group to configure network interface settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void AddNetworkInterfaceCommandGroup(this Command parentCommand)
    {
        var interfaceCommand = parentCommand.AddCommand(
            "interface",
            "Configure a specified network interface.");

        interfaceCommand.AddCommand(s_statusCommand);
        interfaceCommand.AddCommand(s_enableCommand);
        interfaceCommand.AddCommand(s_disableCommand);
        interfaceCommand.AddCommand(s_setMACCommand);
        interfaceCommand.AddIPv4CommandGroup();
    }

    private static async Task StatusHandler(string interfaceName)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        var enabledStateMessage = interfaceToConfigure.CommonInformation.Enabled
            ? CommandLineStrings.NetworkInterfaceIsEnabled
            : CommandLineStrings.NetworkInterfaceIsDisabled;

        SimpleOutput.WriteLine(enabledStateMessage, interfaceName);
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

        if (interfaceToConfigure.CommonInformation.Enabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.NetworkInterfaceAlreadyEnabled
                : CommandLineStrings.NetworkInterfaceAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info, interfaceName);

            return;
        }

        interfaceToConfigure.CommonInformation.Enabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings.NetworkInterfaceEnabled
            : CommandLineStrings.NetworkInterfaceDisabled;

        SimpleOutput.WriteLine(enableMessage, interfaceName);

    }

    private static async Task MACAddressHandler(string interfaceName, string macAddress = "")
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        if (string.IsNullOrWhiteSpace(macAddress))
        {
            new KeyValueOutput()
                .With(new(CommandLineStrings.PhysicalAddress, interfaceToConfigure.CommonInformation.PhysicalAddress))
                .WriteLine();

            return;
        }

        if (macAddress.Length != 12 &&
            macAddress.Length != 17)
        {
            SimpleOutput.WriteLineError(CommandLineStrings.NetworkInterfaceMACInvalidLength, macAddress);

            return;
        }

        macAddress = MACUtilities.ConvertToColonNotation(macAddress);

        if (interfaceToConfigure.CommonInformation.PhysicalAddress.Equals(macAddress))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceMACAlreadyDefined, macAddress);

            return;
        }

        var oldMacAddress = interfaceToConfigure.CommonInformation.PhysicalAddress;
        interfaceToConfigure.CommonInformation.PhysicalAddress = macAddress;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(
            CommandLineStrings.NetworkInterfaceMACSet,
            oldMacAddress,
            macAddress,
            interfaceToConfigure.CommonInformation.Name);
    }
}
