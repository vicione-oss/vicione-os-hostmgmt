using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains set command.
/// </summary>
internal sealed class SearchDomainsSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsSetCommand"/>.
    /// </summary>
    /// <param name="searchDomainsArgument">The search domains argument.</param>
    public SearchDomainsSetCommand(Argument<string[]> searchDomainsArgument) : base(
        CliStrings.Common.SetCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsSetCommandDescription)
    {
        Add(searchDomainsArgument);

        SetAction((parseResult, cancellationToken)
            => SetHandler(parseResult.GetRequiredValue(searchDomainsArgument), cancellationToken));
    }

    private static async Task SetHandler(string[] searchDomains, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.SearchDomains.Domains.Clear();

        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            config.NetworkDNSSettings.SearchDomains.Domains.Add(searchDomain);
            output.With(searchDomain);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_DNS.SearchDomainSet
            : CommandLineStrings_DNS.SearchDomainsSet;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.SearchDomains.Enabled)
            output.WithWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);

        output.WriteLine();
    }
}
