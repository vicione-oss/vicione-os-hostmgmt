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
    public SearchDomainsSetCommand(Argument<string[]> searchDomainsArgument)
        : base(CliStrings.Common.SetCommandName, CliStrings.Network.DNS.SearchDomains.SetCommandDescription)
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
            ? CliStrings.Network.DNS.SearchDomains.Set
            : CliStrings.Network.DNS.SearchDomains.SetMultiple;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.SearchDomains.Enabled)
            output.WithWarning(CliStrings.Network.DNS.SearchDomains.CurrentlyDisabled);

        output.WriteLine();
    }
}
