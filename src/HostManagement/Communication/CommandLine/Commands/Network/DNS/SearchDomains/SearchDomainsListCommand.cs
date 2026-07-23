using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains list command.
/// </summary>
internal sealed class SearchDomainsListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsListCommand"/>.
    /// </summary>
    public SearchDomainsListCommand() : base(
        CommandLineStrings_Common.ListCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsListCommandDescription)
            => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Domains.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainsNotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings_DNS.SearchDomains, config.NetworkDNSSettings.SearchDomains.Enabled)
            .WithEach(config.NetworkDNSSettings.SearchDomains.Domains.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
