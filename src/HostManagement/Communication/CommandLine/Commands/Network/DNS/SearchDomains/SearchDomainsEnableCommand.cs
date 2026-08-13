using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains enable command.
/// </summary>
internal sealed class SearchDomainsEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsEnableCommand"/>.
    /// </summary>
    public SearchDomainsEnableCommand()
        : base(
            CliStrings.Common.EnableCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsEnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainsAlreadyEnabled);
            return;
        }

        if (config.NetworkDNSSettings.SearchDomains.Domains.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.SearchDomainsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.SearchDomains.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SearchDomainsEnabled);
    }
}
