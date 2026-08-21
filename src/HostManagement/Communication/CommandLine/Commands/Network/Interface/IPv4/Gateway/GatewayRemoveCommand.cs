using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public GatewayRemoveCommand()
        : base(CliStrings.Common.RemoveCommandName, CliStrings.Network.Interface.Gateway.RemoveCommandDescription)
        => SetAction((parseResult, cancellationToken) => RemoveHandler(cancellationToken));

    private static async Task RemoveHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var interfaceToConfigure = config.NetworkInterfaces
            .FirstOrDefault(interfaceDetail => interfaceDetail.IPv4.Gateway is not null);

        if (interfaceToConfigure is null)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Interface.Gateway.IsNotConfigured);
            return;
        }

        var oldAddress = interfaceToConfigure.IPv4.Gateway;
        interfaceToConfigure.IPv4.Gateway = null;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Interface.Gateway.Removed, oldAddress);
    }
}
