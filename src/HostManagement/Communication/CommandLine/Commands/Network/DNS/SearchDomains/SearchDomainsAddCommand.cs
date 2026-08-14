using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains add command.
/// </summary>
internal sealed class SearchDomainsAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsAddCommand"/>.
    /// </summary>
    /// <param name="searchDomainsArgument">The search domains argument.</param>
    public SearchDomainsAddCommand(Argument<string[]> searchDomainsArgument)
        : base(CliStrings.Common.AddCommandName, CliStrings.Network.DNS.SearchDomains.AddCommandDescription)
    {
        Add(searchDomainsArgument);

        SetAction((parseResult, cancellationToken)
            => AddHandler(parseResult.GetRequiredValue(searchDomainsArgument), cancellationToken));
    }

    private static async Task AddHandler(string[] searchDomains, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasAddedSearchDomain = false;
        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            if (config.NetworkDNSSettings.SearchDomains.Domains.Contains(searchDomain))
            {
                output.WithInformation(CliStrings.Network.DNS.SearchDomains.AlreadyDefined, searchDomain);
                continue;
            }

            config.NetworkDNSSettings.SearchDomains.Domains.Add(searchDomain);
            output.With(searchDomain);

            hasAddedSearchDomain = true;
        }

        if (hasAddedSearchDomain)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CliStrings.Network.DNS.SearchDomains.Added
                : CliStrings.Network.DNS.SearchDomains.AddedMultiple;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.SearchDomains.Enabled)
                output.WithWarning(CliStrings.Network.DNS.SearchDomains.CurrentlyDisabled);
        }

        output.WriteLine();
    }
}
