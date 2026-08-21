using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public SearchDomainsRemoveCommand(Argument<string[]> searchDomainsArgument)
        : base(CliStrings.Common.RemoveCommandName, CliStrings.Network.DNS.SearchDomains.RemoveCommandDescription)
    {
        Add(searchDomainsArgument);

        SetAction((parseResult, cancellationToken)
            => RemoveHandler(parseResult.GetRequiredValue(searchDomainsArgument), cancellationToken));
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
                output.WithWarning(CliStrings.Network.DNS.SearchDomains.NotFound, searchDomain);
                continue;
            }

            if (!config.NetworkDNSSettings.SearchDomains.Domains.Remove(searchDomain))
            {
                output
                    .WithError(CliStrings.Network.DNS.SearchDomains.CouldNotBeRemoved, searchDomain)
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

                    output.WithInformation(CliStrings.Network.DNS.SearchDomains.Disabled);
                }
            }
            else
            {
                if (config.NetworkDNSSettings.SearchDomains.Domains.Count > 0)
                {
                    output.WithWarning(CliStrings.Network.DNS.SearchDomains.CurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CliStrings.Network.DNS.SearchDomains.Removed
                : CliStrings.Network.DNS.SearchDomains.RemovedMultiple;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}
