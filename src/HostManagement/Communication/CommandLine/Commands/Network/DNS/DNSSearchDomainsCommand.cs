using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the DNS search domains command.
/// </summary>
internal sealed class DNSSearchDomainsCommand : ConfigCommandBase
{
    private static readonly Option<string> s_oldSearchDomainOption = new(["-o", "--old-search-domain"])
    {
        Description = "The old search domain.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string> s_newSearchDomainOption = new(["-n", "--new-search-domain"])
    {
        Description = "The new search domain.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string[]> s_searchDomainItemsOption = CommonOptions.s_itemsOption;

    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsCommand"/>.
    /// </summary>
    public DNSSearchDomainsCommand() : base(
        CommandLineStrings_DNS.DNSSearchDomainsCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsEnableCommandDescription,
            () => EnableDisableHandler(enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsDisableCommandDescription,
            () => EnableDisableHandler(enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsListCommandDescription, ListHandler);

        Add(
            CommandLineStrings_Common.AddCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsAddCommandDescription,
            s_searchDomainItemsOption,
            AddHandler);

        Add(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsRemoveCommandDescription,
            s_searchDomainItemsOption,
            RemoveHandler);

        Add(
            CommandLineStrings_Common.SetCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsSetCommandDescription,
            s_searchDomainItemsOption,
            SetHandler);

        Add(
            CommandLineStrings_Common.ClearCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsClearCommandDescription,
            ClearHandler);

        Add(
            CommandLineStrings_Common.ModifyCommandName,
            CommandLineStrings_DNS.DNSSearchDomainsModifyCommandDescription,
            s_oldSearchDomainOption,
            s_newSearchDomainOption,
            ModifyHandler);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomainsEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_DNS.SearchDomainsAlreadyEnabled
                : CommandLineStrings_DNS.SearchDomainsAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        if (enable &&
            config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.SearchDomainsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.SearchDomainsEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_DNS.SearchDomainsEnabled
            : CommandLineStrings_DNS.SearchDomainsDisabled;

        SimpleOutput.WriteLine(enableMessage);

    }

    private static async Task AddHandler(string[] searchDomains)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedSearchDomain = false;
        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            if (config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                output.WithInformation(CommandLineStrings_DNS.SearchDomainAlreadyDefined, searchDomain);
                continue;
            }

            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            output.With(searchDomain);

            hasAddedSearchDomain = true;
        }

        if (hasAddedSearchDomain)
        {
            if (!await SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.SearchDomainAdded
                : CommandLineStrings_DNS.SearchDomainsAdded;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.SearchDomainsEnabled)
                output.WithWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(string[] searchDomains)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedSearchDomain = false;
        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            if (!config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                output.WithWarning(CommandLineStrings_DNS.SearchDomainNotFound, searchDomain);
                continue;
            }

            if (!config.NetworkDNSSettings.SearchDomains.Remove(searchDomain))
            {
                output
                    .WithError(CommandLineStrings_DNS.SearchDomainCouldNotBeRemoved, searchDomain)
                    .WriteLine();

                return;
            }

            output.With(searchDomain);

            hasRemovedSearchDomain = true;
        }

        if (hasRemovedSearchDomain)
        {
            if (config.NetworkDNSSettings.SearchDomainsEnabled)
            {
                if (config.NetworkDNSSettings.SearchDomains.Count == 0)
                {
                    config.NetworkDNSSettings.SearchDomainsEnabled = false;

                    output.WithInformation(CommandLineStrings_DNS.SearchDomainsDisabled);
                }
            }
            else
            {
                if (config.NetworkDNSSettings.SearchDomains.Count > 0)
                {
                    output.WithWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.SearchDomainRemoved
                : CommandLineStrings_DNS.SearchDomainsRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }

    private static async Task SetHandler(string[] searchDomains)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.SearchDomains.Clear();

        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            output.With(searchDomain);
        }

        if (!await SetConfiguration(config))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_DNS.SearchDomainSet
            : CommandLineStrings_DNS.SearchDomainsSet;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.SearchDomainsEnabled)
            output.WithWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.SearchDomains.Clear();
        config.NetworkDNSSettings.SearchDomainsEnabled = false;

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainsDisabled);
        SimpleOutput.WriteLine(CommandLineStrings_DNS.SearchDomainsCleared);
    }

    private static async Task ModifyHandler(string oldSearchDomain, string newSearchDomain)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
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

        if (!await SetConfiguration(config))
            return;

        if (!config.NetworkDNSSettings.SearchDomainsEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.SearchDomainsCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SearchDomainModified, oldSearchDomain, newSearchDomain);
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainsNotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings_DNS.SearchDomains, config.NetworkDNSSettings.SearchDomainsEnabled)
            .WithEach(config.NetworkDNSSettings.SearchDomains.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
