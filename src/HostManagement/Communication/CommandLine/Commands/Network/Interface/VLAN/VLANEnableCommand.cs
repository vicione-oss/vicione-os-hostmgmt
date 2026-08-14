using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;

/// <summary>
/// Represents the VLAN enable command.
/// </summary>
internal sealed class VLANEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="VLANEnableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public VLANEnableCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.Interface.VLAN.EnableCommandDescription)
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

        if (interfaceToConfigure.VLAN.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Interface.VLAN.AlreadyEnabled, interfaceName);
            return;
        }

        interfaceToConfigure.VLAN.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Interface.VLAN.Enabled, interfaceName);
    }
}
