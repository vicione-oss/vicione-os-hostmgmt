using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains set command.
/// </summary>
internal sealed class DNSSearchDomainsSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsSetCommand"/>.
    /// </summary>
    /// <param name="searchDomainsOption">The search domains option.</param>
    public DNSSearchDomainsSetCommand(Option<string[]> searchDomainsOption) : base(
        CommandLineStrings_Common.SetCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsSetCommandDescription)
    {
        Add(searchDomainsOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return SetHandler(
                parseResult.GetRequiredValue(searchDomainsOption),
                cancellationToken);
        });
    }

    private static async Task SetHandler(string[] searchDomains, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.SearchDomains.Clear();

        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            output.With(searchDomain);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_DNS.SearchDomainSet
            : CommandLineStrings_DNS.SearchDomainsSet;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.SearchDomainsEnabled)
            output.WithWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);

        output.WriteLine();
    }
}
