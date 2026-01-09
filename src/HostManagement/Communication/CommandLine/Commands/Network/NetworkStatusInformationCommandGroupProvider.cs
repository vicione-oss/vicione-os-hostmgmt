using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts.NetworkStatus;

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
        var networkStatusCommand = parentCommand.AddCommand("status", "Get network status information.");
        networkStatusCommand.SetHandler(NetworkTestHandler);
    }

    private static async Task NetworkTestHandler()
    {
        if (await NamedPipeRequestHelper.GetNetworkStatusInformation() is not NetworkStatusInformation networkStatus)
            return;

        var builder = new CommandLineStringBuilder();

        builder.AppendLine($"Default route configured: {networkStatus.IsDefaultRouteConfigured}");
        builder.AppendLine($"Default gateway available: {networkStatus.IsDefaultGatewayAvailable}");
        builder.AppendLine($"Internet available: {networkStatus.IsInternetAvailable}");
        builder.AppendLine($"DNS functional: {networkStatus.IsDNSFunctional}");
        builder.AppendLine("Interfaces");

        if (networkStatus.Interfaces.Count == 0)
        {
            builder.AppendLine(1, "None");
            Console.WriteLine(builder.ToString());

            return;
        }

        var maxNameLength = networkStatus.Interfaces
            .Max(interfaceInformation => interfaceInformation.Name.Length);

        foreach (var item in networkStatus.Interfaces)
            builder.AppendLine(1, $"Name: {item.Name.PadRight(maxNameLength)}  State: {item.State}");

        Console.WriteLine(builder.ToString());
    }
}
