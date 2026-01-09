using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.DNS;
using HostManagement.Communication.CommandLine.Commands.Network.Interface;
using HostManagement.Communication.CommandLine.Commands.Network.NTP;
using HostManagement.Communication.CommandLine.Commands.Network.Proxy;

namespace HostManagement.Communication.CommandLine.Commands.Network;

/// <summary>
/// Represents the network command.
/// </summary>
internal sealed class NetworkCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="NetworkCommand"/>.
    /// </summary>
    public NetworkCommand() : base(
        CommandLineStrings_Network.NetworkCommandName,
        CommandLineStrings_Network.NetworkCommandDescription)
    {
        Add(new ListInterfacesCommand());
        Add(new NetworkStatusCommand());
        Add(new InterfaceCommand());
        Add(new DNSCommand());
        Add(new ProxyCommand());
        Add(new NTPCommand());
    }
}
