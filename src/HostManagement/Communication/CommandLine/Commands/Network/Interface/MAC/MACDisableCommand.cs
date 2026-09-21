using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.MAC;

/// <summary>
/// Represents the MAC disable command.
/// </summary>
internal sealed class MACDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="MACDisableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public MACDisableCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.Interface.MAC.DisableCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => DisableHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task DisableHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        var userDefinedMACAddress = interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Address;

        if (!interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Enabled)
        {
            SimpleOutput.WriteLineInformation(
                CliStrings.Network.Interface.MAC.AlreadyDisabled,
                userDefinedMACAddress,
                interfaceName);

            return;
        }

        interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(
            CliStrings.Network.Interface.MAC.Disabled,
            userDefinedMACAddress,
            interfaceName);
    }
}
