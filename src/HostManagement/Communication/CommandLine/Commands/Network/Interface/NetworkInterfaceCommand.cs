using System.CommandLine;
using HostManagement.Common.Utilities;
using HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface command.
/// </summary>
internal sealed class NetworkInterfaceCommand : ConfigCommandBase
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;

    internal static readonly Option<string> s_macAddressOption = new(["-a", "--address"])
    {
        Description = "The MAC address of the specified network interface.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="NetworkInterfaceCommand"/>.
    /// </summary>
    public NetworkInterfaceCommand() : base(
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceCommandName,
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_NetworkInterfaces.NetworkInterfaceEnableCommandDescription,
            s_nameOption,
            (name) => EnableDisableHandler(interfaceName: name, enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_NetworkInterfaces.NetworkInterfaceDisableCommandDescription,
            s_nameOption,
            (name) => EnableDisableHandler(interfaceName: name, enable: false));

        Add(
            CommandLineStrings_Common.StatusCommandName,
            CommandLineStrings_NetworkInterfaces.NetworkInterfaceStatusCommandDescription,
            s_nameOption,
            StatusHandler);

        Add(
            CommandLineStrings_NetworkInterfaces.NetworkInterfaceMACCommandName,
            CommandLineStrings_NetworkInterfaces.NetworkInterfaceMACCommandDescription,
            s_nameOption,
            s_macAddressOption,
            MACAddressHandler);

        Add(new IPv4Command());
    }

    private static async Task StatusHandler(string interfaceName)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);

            return;
        }

        var enabledStateMessage = interfaceToConfigure.CommonInformation.Enabled
            ? CommandLineStrings_NetworkInterfaces.InterfaceIsEnabled
            : CommandLineStrings_NetworkInterfaces.InterfaceIsDisabled;

        SimpleOutput.WriteLine(enabledStateMessage, interfaceName);
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

        if (interfaceToConfigure.CommonInformation.Enabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_NetworkInterfaces.InterfaceAlreadyEnabled
                : CommandLineStrings_NetworkInterfaces.InterfaceAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info, interfaceName);

            return;
        }

        interfaceToConfigure.CommonInformation.Enabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_NetworkInterfaces.InterfaceEnabled
            : CommandLineStrings_NetworkInterfaces.InterfaceDisabled;

        SimpleOutput.WriteLine(enableMessage, interfaceName);

    }

    private static async Task MACAddressHandler(string interfaceName, string macAddress = "")
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);

            return;
        }

        if (string.IsNullOrWhiteSpace(macAddress))
        {
            var physicalAddress = interfaceToConfigure.CommonInformation.PhysicalAddress;

            var originalPhysicalAddress = await NamedPipeRequestHelper
                .GetOriginalPhysicalAddress(interfaceToConfigure.CommonInformation.Name);

            originalPhysicalAddress ??= CommandLineStrings_NetworkInterfaces.Unknown;

            var output = new KeyValueOutput()
                .With(new(CommandLineStrings_NetworkInterfaces.PhysicalAddress, physicalAddress));

            if (!physicalAddress.Equals(originalPhysicalAddress))
                output.With(new(CommandLineStrings_NetworkInterfaces.OriginalPhysicalAddress, originalPhysicalAddress));

            output.WriteLine();

            return;
        }

        if (macAddress.Length != 12 &&
            macAddress.Length != 17)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_NetworkInterfaces.InterfaceMACInvalidLength, macAddress);

            return;
        }

        macAddress = MACUtilities.ConvertToColonNotation(macAddress);

        if (interfaceToConfigure.CommonInformation.PhysicalAddress.Equals(macAddress))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceMACAlreadyDefined, macAddress);

            return;
        }

        var oldMacAddress = interfaceToConfigure.CommonInformation.PhysicalAddress;
        interfaceToConfigure.CommonInformation.PhysicalAddress = macAddress;

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(
            CommandLineStrings_NetworkInterfaces.InterfaceMACSet,
            oldMacAddress,
            macAddress,
            interfaceToConfigure.CommonInformation.Name);
    }
}
