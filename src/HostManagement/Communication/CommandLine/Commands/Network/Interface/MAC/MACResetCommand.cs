using System.CommandLine;
using System.Net.NetworkInformation;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.MAC;

/// <summary>
/// Represents the MAC reset command.
/// </summary>
internal sealed class MACResetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="MACResetCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public MACResetCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.ResetCommandName, CliStrings.Network.Interface.MAC.ResetCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => ResetHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task ResetHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        var originalMACAddress = await NamedPipeRequestHelper
            .GetOriginalPhysicalAddress(interfaceName, cancellationToken);

        if (originalMACAddress is null)
            return;

        interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Enabled = false;
        interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Address = PhysicalAddress.None;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Interface.MAC.ResetSucceeded, interfaceName, originalMACAddress);
    }
}
