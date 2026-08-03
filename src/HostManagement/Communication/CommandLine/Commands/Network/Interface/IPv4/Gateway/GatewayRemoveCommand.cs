using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4.Gateway;

/// <summary>
/// Represents the gateway remove command.
/// </summary>
internal sealed class GatewayRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="GatewayRemoveCommand"/>.
    /// </summary>
    public GatewayRemoveCommand() : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_NetworkInterfaces.GatewayRemoveCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => RemoveHandler(cancellationToken));
    }

    private static async Task RemoveHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var interfaceToConfigure = config.NetworkInterfaces
            .FirstOrDefault(interfaceDetail => interfaceDetail.IPv4.Gateway is not null);

        if (interfaceToConfigure is null)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.GatewayIsNotConfigured);
            return;
        }

        var oldAddress = interfaceToConfigure.IPv4.Gateway;
        interfaceToConfigure.IPv4.Gateway = null;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.GatewayRemoved, oldAddress);
    }
}
