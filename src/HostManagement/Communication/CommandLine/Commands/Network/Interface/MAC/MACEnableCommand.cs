using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.MAC;

/// <summary>
/// Represents the MAC enable command.
/// </summary>
internal sealed class MACEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="MACEnableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public MACEnableCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.Interface.MAC.EnableCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => EnableHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task EnableHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        var userDefinedMACAddress = interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Address;

        if (interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Enabled)
        {
            SimpleOutput.WriteLineInformation(
                CliStrings.Network.Interface.MAC.AlreadyEnabled,
                userDefinedMACAddress,
                interfaceName);

            return;
        }

        interfaceToConfigure.CommonInformation.UserDefinedMACAddress.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(
            CliStrings.Network.Interface.MAC.Enabled,
            userDefinedMACAddress,
            interfaceName);
    }
}
