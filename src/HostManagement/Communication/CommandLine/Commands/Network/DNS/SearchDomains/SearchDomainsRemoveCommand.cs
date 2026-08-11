using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains remove command.
/// </summary>
internal sealed class SearchDomainsRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsRemoveCommand"/>.
    /// </summary>
    /// <param name="searchDomainsArgument">The search domains argument.</param>
    public SearchDomainsRemoveCommand(Argument<string[]> searchDomainsArgument) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsRemoveCommandDescription)
    {
        Add(searchDomainsArgument);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(searchDomainsArgument),
                cancellationToken);
        });
    }

    private static async Task RemoveHandler(string[] searchDomains, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasRemovedSearchDomain = false;
        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            if (!config.NetworkDNSSettings.SearchDomains.Domains.Contains(searchDomain))
            {
                output.WithWarning(CommandLineStrings_DNS.SearchDomainNotFound, searchDomain);
                continue;
            }

            if (!config.NetworkDNSSettings.SearchDomains.Domains.Remove(searchDomain))
            {
                output
                    .WithError(CommandLineStrings_DNS.SearchDomainCouldNotBeRemoved, searchDomain)
                    .WriteLine();

                return;
            }

            output.With(searchDomain);

            hasRemovedSearchDomain = true;
        }

        if (hasRemovedSearchDomain)
        {
            if (config.NetworkDNSSettings.SearchDomains.Enabled)
            {
                if (config.NetworkDNSSettings.SearchDomains.Domains.Count == 0)
                {
                    config.NetworkDNSSettings.SearchDomains.Enabled = false;

                    output.WithInformation(CommandLineStrings_DNS.SearchDomainsDisabled);
                }
            }
            else
            {
                if (config.NetworkDNSSettings.SearchDomains.Domains.Count > 0)
                {
                    output.WithWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.SearchDomainRemoved
                : CommandLineStrings_DNS.SearchDomainsRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}
