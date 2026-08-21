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
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.DNS.SearchDomains.EnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.SearchDomains.AlreadyEnabled);
            return;
        }

        if (config.NetworkDNSSettings.SearchDomains.Domains.Count == 0)
        {
            SimpleOutput.WriteLineError(CliStrings.Network.DNS.SearchDomains.NotConfigured);
            return;
        }

        config.NetworkDNSSettings.SearchDomains.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.SearchDomains.Enabled);
    }
}
