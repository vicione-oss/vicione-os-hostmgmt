using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains modify command.
/// </summary>
internal sealed class DNSSearchDomainsModifyCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsModifyCommand"/>.
    /// </summary>
    /// <param name="oldSearchDomainOption">The old search domain option.</param>
    /// <param name="newSearchDomainOption">The new search domain option.</param>
    public DNSSearchDomainsModifyCommand(
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

    private static async Task ModifyHandler(
        string oldSearchDomain,
        string newSearchDomain,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var searchDomains = config.NetworkDNSSettings.SearchDomains;

        if (searchDomains.Contains(newSearchDomain))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainAlreadyDefined, newSearchDomain);
            return;
        }

        var index = searchDomains.IndexOf(oldSearchDomain);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.SearchDomainNotFound, oldSearchDomain);
            return;
        }

        searchDomains[index] = newSearchDomain;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (!config.NetworkDNSSettings.SearchDomainsEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SearchDomainModified, oldSearchDomain, newSearchDomain);
    }
}
