using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the network interface enable command.
/// </summary>
internal sealed class InterfaceEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="InterfaceEnableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public InterfaceEnableCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.Interface.EnableCommandDescription)
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

        if (interfaceToConfigure.CommonInformation.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Interface.AlreadyEnabled, interfaceName);
            return;
        }

        interfaceToConfigure.CommonInformation.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Interface.Enabled, interfaceName);
    }
}
