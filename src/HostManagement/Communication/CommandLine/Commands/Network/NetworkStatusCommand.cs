using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Interface;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
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
    public NetworkStatusCommand(Argument<string> interfaceNameArgument) : base(
        CommandLineStrings_Common.StatusCommandName,
        CommandLineStrings_Network.NetworkStatusCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken) =>
        {
            return NetworkTestHandler(
                parseResult.GetRequiredValue(interfaceNameArgument),
                cancellationToken);
        });
    }

    private static async Task NetworkTestHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is null)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        var networkStatus = await NamedPipeRequestHelper.GetNetworkStatusInformation(interfaceName, cancellationToken);

        if (networkStatus is null)
            return;

        new KeyValueOutput()
            .With(new(CommandLineStrings_Network.InterfaceState, networkStatus.InterfaceState))
            .With(new(CommandLineStrings_Network.DefaultRouteConfigured, networkStatus.IsDefaultRouteConfigured))
            .With(new(CommandLineStrings_Network.DefaultGatewayAvailable, networkStatus.IsDefaultGatewayAvailable))
            .With(new(CommandLineStrings_Network.InternetAvailable, networkStatus.IsInternetAvailable))
            .With(new(CommandLineStrings_Network.DNSFunctional, networkStatus.IsDNSFunctional))
            .WriteLine();
    }
}
