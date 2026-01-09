using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Multicast;

/// <summary>
/// Represents the DNS multicast command.
/// </summary>
internal sealed class DNSMulticastCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="DNSMulticastCommand"/>.
    /// </summary>
    public DNSMulticastCommand() : base(
        CommandLineStrings_DNS.DNSMulticastCommandName,
        CommandLineStrings_DNS.DNSMulticastCommandDescription)
    {
        Add(new DNSMulticastEnableCommand());
        Add(new DNSMulticastDisableCommand());
        Add(new DNSMulticastStatusCommand());
    }
}
