using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains remove command.
/// </summary>
internal sealed class DNSSearchDomainsRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsRemoveCommand"/>.
    /// </summary>
    /// <param name="searchDomainsOption">The search domains option.</param>
    public DNSSearchDomainsRemoveCommand(Option<string[]> searchDomainsOption) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsRemoveCommandDescription)
    {
        Add(searchDomainsOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(searchDomainsOption),
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
            if (!config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                output.WithWarning(CommandLineStrings_DNS.SearchDomainNotFound, searchDomain);
                continue;
            }

            if (!config.NetworkDNSSettings.SearchDomains.Remove(searchDomain))
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
            if (config.NetworkDNSSettings.SearchDomainsEnabled)
            {
                if (config.NetworkDNSSettings.SearchDomains.Count == 0)
                {
                    config.NetworkDNSSettings.SearchDomainsEnabled = false;

                    output.WithInformation(CommandLineStrings_DNS.SearchDomainsDisabled);
                }
            }
            else
            {
                if (config.NetworkDNSSettings.SearchDomains.Count > 0)
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
