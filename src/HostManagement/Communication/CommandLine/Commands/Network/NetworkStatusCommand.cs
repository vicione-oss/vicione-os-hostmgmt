using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network;

/// <summary>
/// Represents the network status command.
/// </summary>
internal sealed class NetworkStatusCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NetworkStatusCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public NetworkStatusCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.StatusCommandName, CliStrings.Network.StatusCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => NetworkTestHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task NetworkTestHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is null)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        var networkStatus = await NamedPipeRequestHelper.GetNetworkStatusInformation(interfaceName, cancellationToken);

        if (networkStatus is null)
            return;

        new KeyValueOutput()
            .With(new(CliStrings.Network.InterfaceState, networkStatus.InterfaceState))
            .With(new(CliStrings.Network.DefaultRouteConfigured, networkStatus.IsDefaultRouteConfigured))
            .With(new(CliStrings.Network.DefaultGatewayAvailable, networkStatus.IsDefaultGatewayAvailable))
            .With(new(CliStrings.Network.InternetAvailable, networkStatus.IsInternetAvailable))
            .With(new(CliStrings.Network.DNSFunctional, networkStatus.IsDNSFunctional))
            .WriteLine();
    }
}
