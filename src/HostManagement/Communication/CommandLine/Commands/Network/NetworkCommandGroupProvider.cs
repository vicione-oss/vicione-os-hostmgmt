using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.DNS;
using HostManagement.Communication.CommandLine.Commands.Network.Interface;
using HostManagement.Communication.CommandLine.Commands.Network.NTP;
using HostManagement.Communication.CommandLine.Commands.Network.Proxy;
using HostManagement.Communication.CommandLine.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network;

/// <summary>
/// Represents the <see cref="NetworkCommandGroupProvider"/>.
/// </summary>
internal static class NetworkCommandGroupProvider
{
    /// <summary>
    /// Adds the network command group.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddNetworkCommandGroup(this Command parentCommand)
    {
        var networkCommand = parentCommand.AddCommand("network", "Manage network settings.");
        networkCommand.AddListNetworkInterfacesCommandGroup();
        networkCommand.AddNetworkStatusInformationCommandGroup();
        networkCommand.AddNetworkInterfaceCommandGroup();
        networkCommand.AddDNSCommandGroup();
        networkCommand.AddProxyCommandGroup();
        networkCommand.AddNTPCommandGroup();
    }
}
