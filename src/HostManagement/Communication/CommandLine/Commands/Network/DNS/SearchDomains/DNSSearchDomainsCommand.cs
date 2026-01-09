using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains command.
/// </summary>
internal sealed class DNSSearchDomainsCommand : Command
{
    private static readonly Option<string> s_oldSearchDomainOption = new("--old-search-domain", "-o")
    {
        Description = "The old search domain.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string> s_newSearchDomainOption = new("--new-search-domain", "-n")
    {
        Description = "The new search domain.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsCommand"/>.
    /// </summary>
    public DNSSearchDomainsCommand() : base(
        CommandLineStrings_DNS.DNSSearchDomainsCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsCommandDescription)
    {
        Add(new DNSSearchDomainsEnableCommand());
        Add(new DNSSearchDomainsDisableCommand());
        Add(new DNSSearchDomainsListCommand());
        Add(new DNSSearchDomainsAddCommand(CommonOptions.s_itemsOption));
        Add(new DNSSearchDomainsRemoveCommand(CommonOptions.s_itemsOption));
        Add(new DNSSearchDomainsSetCommand(CommonOptions.s_itemsOption));
        Add(new DNSSearchDomainsClearCommand());
        Add(new DNSSearchDomainsModifyCommand(s_oldSearchDomainOption, s_newSearchDomainOption));
    }
}
