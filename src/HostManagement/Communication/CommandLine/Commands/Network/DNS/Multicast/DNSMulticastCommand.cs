using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Multicast;

/// <summary>
/// Represents the DNS multicast command.
/// </summary>
internal sealed class DNSMulticastCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="DNSMulticastCommand"/>.
    /// </summary>
    public DNSMulticastCommand()
        : base(CliStrings.Network.DNS.Multicast.CommandName, CliStrings.Network.DNS.Multicast.CommandDescription)
    {
        Add(new DNSMulticastStatusCommand());
        Add(new DNSMulticastEnableCommand());
        Add(new DNSMulticastDisableCommand());
    }
}
