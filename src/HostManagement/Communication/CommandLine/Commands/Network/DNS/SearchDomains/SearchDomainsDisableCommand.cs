using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains disable command.
/// </summary>
internal sealed class SearchDomainsDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsDisableCommand"/>.
    /// </summary>
    public SearchDomainsDisableCommand()
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.DNS.SearchDomains.DisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.SearchDomains.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.SearchDomains.AlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.SearchDomains.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.SearchDomains.Disabled);
    }
}
