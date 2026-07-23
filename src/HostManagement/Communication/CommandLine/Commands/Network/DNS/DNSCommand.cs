using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.DNS.Hostname;
using HostManagement.Communication.CommandLine.Commands.Network.DNS.Multicast;
using HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;
using HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;
using HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;
using HostManagement.Communication.CommandLine.Commands.Network.DNS.Suffix;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the DNS command.
/// </summary>
internal sealed class DNSCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="DNSCommand"/>.
    /// </summary>
    public DNSCommand() : base(
        CommandLineStrings_DNS.DNSCommandName,
        CommandLineStrings_DNS.DNSCommandDescription)
    {
        Add(new DNSHostnameCommand());
        Add(new DNSMulticastCommand());
        Add(new StaticHostsCommand());
        Add(new NameServersCommand());
        Add(new DNSSuffixCommand());
        Add(new DNSSearchDomainsCommand());
    }
}
