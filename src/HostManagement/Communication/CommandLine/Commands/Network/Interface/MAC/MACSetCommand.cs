using System.CommandLine;
using HostManagement.Common.Utilities;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.MAC;

/// <summary>
/// Represents the MAC set command.
/// </summary>
internal sealed class MACSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="MACSetCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    /// <param name="macAddressOption">The MAC address option.</param>
    public MACSetCommand(Option<string> interfaceNameOption, Option<string> macAddressOption)
        : base(CliStrings.Common.SetCommandName, CliStrings.Network.Interface.MAC.SetCommandDescription)
    {
        Add(interfaceNameOption);
        Add(macAddressOption);

        SetAction((parseResult, cancellationToken)
            => SetHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                parseResult.GetRequiredValue(macAddressOption),
                cancellationToken));
    }

    private static async Task SetHandler(string interfaceName, string macAddress, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        if (macAddress.Length is not 12 and not 17)
        {
            SimpleOutput.WriteLineError(CliStrings.Network.Interface.MAC.InvalidLength, macAddress);
            return;
        }

        macAddress = MACUtilities.ConvertToColonNotation(macAddress);

        var originalMACAddress = await NamedPipeRequestHelper
            .GetOriginalPhysicalAddress(interfaceName, cancellationToken);

        if (originalMACAddress is null)
            return;

        var oldUserDefinedMacAddress = interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Address;

        if (oldUserDefinedMacAddress.Equals(macAddress))
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.MAC.AlreadyDefined, macAddress);
            return;
        }

        if (originalMACAddress.Equals(macAddress))
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.MAC.IdenticalToOriginalAddress, macAddress);
            return;
        }

        interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Enabled = true;
        interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Address = macAddress;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(
            CliStrings.Network.Interface.MAC.Set,
            oldUserDefinedMacAddress,
            macAddress,
            interfaceToConfigure.CommonInformation.Name);
    }
}
