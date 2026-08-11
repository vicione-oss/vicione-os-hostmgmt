using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains command.
/// </summary>
internal sealed class SearchDomainsCommand : Command
{
    private static readonly Argument<string[]> s_searchDomainsArgument = new("domains")
    {
        Description = "The list of search domains.",
        Arity = ArgumentArity.OneOrMore
    };

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
    /// Initializes a new <see cref="SearchDomainsCommand"/>.
    /// </summary>
    public SearchDomainsCommand() : base(
        CommandLineStrings_DNS.DNSSearchDomainsCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsCommandDescription)
    {
        Add(new SearchDomainsListCommand());
        Add(new SearchDomainsEnableCommand());
        Add(new SearchDomainsDisableCommand());
        Add(new SearchDomainsAddCommand(s_searchDomainsArgument));
        Add(new SearchDomainsRemoveCommand(s_searchDomainsArgument));
        Add(new SearchDomainsSetCommand(s_searchDomainsArgument));
        Add(new SearchDomainsClearCommand());
        Add(new SearchDomainsModifyCommand(s_oldSearchDomainOption, s_newSearchDomainOption));
    }
}
