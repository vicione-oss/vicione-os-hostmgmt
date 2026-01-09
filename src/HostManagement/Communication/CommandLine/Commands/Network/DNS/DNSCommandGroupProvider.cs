using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="DNSCommandGroupProvider"/>.
/// </summary>
internal static class DNSCommandGroupProvider
{
    /// <summary>
    /// Adds the command group to configure DNS settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddDNSCommandGroup(this Command parentCommand)
    {
        var dnsCommand = parentCommand.AddCommand(
            "dns",
            "Configure DNS settings.");

        dnsCommand.AddHostnameCommandGroup();
        dnsCommand.AddMulticastDNSCommandGroup();
        dnsCommand.AddStaticHostsCommandGroup();
        dnsCommand.AddDNSNameServersCommandGroup();
        dnsCommand.AddDNSSuffixCommandGroup();
        dnsCommand.AddDNSSearchDomainsCommandGroup();
    }
}
