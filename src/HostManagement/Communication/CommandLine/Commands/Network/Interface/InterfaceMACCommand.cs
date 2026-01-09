using System.CommandLine;
using HostManagement.Common.Utilities;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface mac command.
/// </summary>
internal sealed class InterfaceMACCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="InterfaceMACCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    /// <param name="macAddressOption">The MAC address option.</param>
    public InterfaceMACCommand(Option<string> interfaceNameOption, Option<string> macAddressOption) : base(
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceMACCommandName,
        CommandLineStrings_NetworkInterfaces.NetworkInterfaceMACCommandDescription)
    {
        Add(interfaceNameOption);
        Add(macAddressOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return MACAddressHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                parseResult.GetValue(macAddressOption) ?? string.Empty,
                cancellationToken);
        });
    }

    private static async Task MACAddressHandler(
        string interfaceName,
        string macAddress,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
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

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(
            CommandLineStrings_NetworkInterfaces.InterfaceMACSet,
            oldMacAddress,
            macAddress,
            interfaceToConfigure.CommonInformation.Name);
    }
}
