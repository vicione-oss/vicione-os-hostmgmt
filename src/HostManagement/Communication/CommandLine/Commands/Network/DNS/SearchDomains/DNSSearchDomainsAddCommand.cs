using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains add command.
/// </summary>
internal sealed class DNSSearchDomainsAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsAddCommand"/>.
    /// </summary>
    /// <param name="searchDomainsOption">The search domains option.</param>
    public DNSSearchDomainsAddCommand(Option<string[]> searchDomainsOption) : base(
        CommandLineStrings_Common.AddCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsAddCommandDescription)
    {
        Add(searchDomainsOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return AddHandler(
                parseResult.GetRequiredValue(searchDomainsOption),
                cancellationToken);
        });
    }

    private static async Task AddHandler(string[] searchDomains, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasAddedSearchDomain = false;
        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            if (config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                output.WithInformation(CommandLineStrings_DNS.SearchDomainAlreadyDefined, searchDomain);
                continue;
            }

            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            output.With(searchDomain);

            hasAddedSearchDomain = true;
        }

        if (hasAddedSearchDomain)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.SearchDomainAdded
                : CommandLineStrings_DNS.SearchDomainsAdded;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.SearchDomainsEnabled)
                output.WithWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);
        }

        output.WriteLine();
    }
}
