using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network;

/// <summary>
/// Represents the <see cref="NetworkStatusInformationCommandGroupProvider"/>.
/// </summary>
internal static class NetworkStatusInformationCommandGroupProvider
{
    /// <summary>
    /// Adds the network status information command group.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddNetworkStatusInformationCommandGroup(this Command parentCommand)
    {
        var networkStatusCommand = parentCommand.AddCommand(
            "status",
            "Get network status information of the specified interface.");

        var interfaceNameArgument = new Argument<string>("name")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        networkStatusCommand.AddArgument(interfaceNameArgument);
        networkStatusCommand.SetHandler(NetworkTestHandler, interfaceNameArgument);
    }

    private static async Task NetworkTestHandler(string interfaceName)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is null)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        if (await NamedPipeRequestHelper.GetNetworkStatusInformation(interfaceName) is not NetworkStatusInformation networkStatus)
            return;

        new KeyValueOutput()
            .With(new(CommandLineStrings.InterfaceState, networkStatus.InterfaceState))
            .With(new(CommandLineStrings.DefaultRouteConfigured, networkStatus.IsDefaultRouteConfigured))
            .With(new(CommandLineStrings.DefaultGatewayAvailable, networkStatus.IsDefaultGatewayAvailable))
            .With(new(CommandLineStrings.InternetAvailable, networkStatus.IsInternetAvailable))
            .With(new(CommandLineStrings.DNSFunctional, networkStatus.IsDNSFunctional))
            .WriteLine();
    }
}
