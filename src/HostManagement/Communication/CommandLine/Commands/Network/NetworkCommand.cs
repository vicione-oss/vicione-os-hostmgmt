using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.DNS;
using HostManagement.Communication.CommandLine.Commands.Network.Interface;
using HostManagement.Communication.CommandLine.Commands.Network.NTP;
using HostManagement.Communication.CommandLine.Commands.Network.Proxy;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network;

/// <summary>
/// Represents the network command.
/// </summary>
internal sealed class NetworkCommand : Command
{
    private static readonly Argument<string> s_interfaceNameArgument = new("name")
    {
        Description = CommandLineStrings_NetworkInterfaces.InterfaceNameArgumentDescription,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="NetworkCommand"/>.
    /// </summary>
    public NetworkCommand() : base(CliStrings.Network.CommandName, CliStrings.Network.CommandDescription)
    {
        Add(new ListInterfacesCommand());
        Add(new NetworkStatusCommand(s_interfaceNameArgument));
        Add(new InterfaceCommand(s_interfaceNameArgument));
        Add(new DNSCommand());
        Add(new ProxyCommand());
        Add(new NTPCommand());
    }
}
