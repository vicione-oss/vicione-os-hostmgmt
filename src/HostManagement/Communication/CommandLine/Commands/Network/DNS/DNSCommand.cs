namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the DNS command.
/// </summary>
internal sealed class DNSCommand : CommandBase
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
        Add(new DNSStaticHostsCommand());
        Add(new DNSNameServersCommand());
        Add(new DNSSuffixCommand());
        Add(new DNSSearchDomainsCommand());
    }
}
