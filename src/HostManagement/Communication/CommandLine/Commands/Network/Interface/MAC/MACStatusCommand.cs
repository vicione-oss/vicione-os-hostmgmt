using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.MAC;

/// <summary>
/// Represents the MAC status command.
/// </summary>
internal sealed class MACStatusCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="MACStatusCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public MACStatusCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.StatusCommandName, CliStrings.Network.Interface.MAC.StatusCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => StatusHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task StatusHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail @interface)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        var userDefinedMACAddressEnabled = @interface.CommonInformation.UserDefinedMACAddress.Enabled;
        var userDefinedMACAddress = @interface.CommonInformation.UserDefinedMACAddress.Address;

        var originalMACAddress = await NamedPipeRequestHelper
            .GetOriginalPhysicalAddress(interfaceName, cancellationToken);

        if (originalMACAddress is null)
            return;

        new KeyValueOutput()
            .WithTitle(CliStrings.Network.Interface.MAC.UserDefinedAddress)
            .With(new(CliStrings.Common.EnabledLabel, userDefinedMACAddressEnabled))
            .With(new("Address", userDefinedMACAddress))
            .WriteLine();

        new KeyValueOutput()
            .WithTitle(CliStrings.Network.Interface.MAC.OriginalAddress)
            .With(new(CliStrings.Common.EnabledLabel, !userDefinedMACAddressEnabled))
            .With(new("Address", originalMACAddress))
            .WriteLine();

        return;
    }
}
