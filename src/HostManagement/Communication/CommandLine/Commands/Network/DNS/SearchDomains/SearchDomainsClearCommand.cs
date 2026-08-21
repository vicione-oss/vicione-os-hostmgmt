using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains clear command.
/// </summary>
internal sealed class SearchDomainsClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsClearCommand"/>.
    /// </summary>
    public SearchDomainsClearCommand()
        : base(CliStrings.Common.ClearCommandName, CliStrings.Network.DNS.SearchDomains.ClearCommandDescription)
        => SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Domains.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.SearchDomains.NotConfigured);
            return;
        }

        config.NetworkDNSSettings.SearchDomains.Domains.Clear();
        config.NetworkDNSSettings.SearchDomains.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.SearchDomains.Disabled);
        SimpleOutput.WriteLine(CliStrings.Network.DNS.SearchDomains.Cleared);
    }
}
