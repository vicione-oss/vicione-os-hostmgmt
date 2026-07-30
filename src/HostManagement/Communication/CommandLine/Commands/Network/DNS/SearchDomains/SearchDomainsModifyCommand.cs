using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains modify command.
/// </summary>
internal sealed class SearchDomainsModifyCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SearchDomainsModifyCommand"/>.
    /// </summary>
    /// <param name="oldSearchDomainOption">The old search domain option.</param>
    /// <param name="newSearchDomainOption">The new search domain option.</param>
    public SearchDomainsModifyCommand(
        Option<string> oldSearchDomainOption,
        Option<string> newSearchDomainOption)
        : base(
            CommandLineStrings_Common.ModifyCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsModifyCommandDescription)
    {
        Add(oldSearchDomainOption);
        Add(newSearchDomainOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return ModifyHandler(
                parseResult.GetRequiredValue(oldSearchDomainOption),
                parseResult.GetRequiredValue(newSearchDomainOption),
                cancellationToken);
        });
    }

    private static async Task ModifyHandler(string oldDomain, string newDomain, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var domains = config.NetworkDNSSettings.SearchDomains.Domains;

        if (domains.Contains(newDomain))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainAlreadyDefined, newDomain);
            return;
        }

        var index = domains.IndexOf(oldDomain);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.SearchDomainNotFound, oldDomain);
            return;
        }

        domains[index] = newDomain;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (!config.NetworkDNSSettings.SearchDomains.Enabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SearchDomainModified, oldDomain, newDomain);
    }
}
