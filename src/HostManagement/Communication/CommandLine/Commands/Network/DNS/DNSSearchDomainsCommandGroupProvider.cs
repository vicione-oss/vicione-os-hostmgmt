using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="DNSSearchDomainsCommandGroupProvider"/>.
/// </summary>
internal static class DNSSearchDomainsCommandGroupProvider
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

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable DNS search domains.",
        () => EnableDisableHandler(enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable DNS search domains.",
        () => EnableDisableHandler(enable: false));

    private static readonly Command s_addCommand = CommandHelper.ConstructCommand(
        "add",
        "Add new DNS search domains.",
        s_searchDomainItemsOption,
        AddHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove existing DNS search domains.",
        s_searchDomainItemsOption,
        RemoveHandler);

    private static readonly Command s_setCommand = CommandHelper.ConstructCommand(
        "set",
        "Remove existing DNS search domains and set new ones.",
        s_searchDomainItemsOption,
        SetHandler);

    private static readonly Command s_clearCommand = CommandHelper.ConstructCommand(
        "clear",
        "Remove all DNS search domains.",
        ClearHandler);

    private static readonly Command s_modifyCommand = CommandHelper.ConstructCommand(
        "modify",
        "Modify an existing DNS search domain.",
        s_oldSearchDomainOption,
        s_newSearchDomainOption,
        ModifyHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List all DNS search domains.",
        ListHandler);

    /// <summary>
    /// Adds the command group to configure DNS search domains.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddDNSSearchDomainsCommandGroup(this Command parentCommand)
    {
        var searchDomainsCommand = parentCommand.AddCommand(
            "search-domains",
            "Configure DNS search domains.");

        searchDomainsCommand.AddCommand(s_enableCommand);
        searchDomainsCommand.AddCommand(s_disableCommand);
        searchDomainsCommand.AddCommand(s_addCommand);
        searchDomainsCommand.AddCommand(s_removeCommand);
        searchDomainsCommand.AddCommand(s_setCommand);
        searchDomainsCommand.AddCommand(s_clearCommand);
        searchDomainsCommand.AddCommand(s_modifyCommand);
        searchDomainsCommand.AddCommand(s_listCommand);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomainsEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.DNSSearchDomainsAlreadyEnabled
                : CommandLineStrings.DNSSearchDomainsAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        config.NetworkDNSSettings.SearchDomainsEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (enable &&
            config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.DNSSearchDomainsNotConfigured);
        }

        var enableMessage = enable
            ? CommandLineStrings.DNSSearchDomainsEnabled
            : CommandLineStrings.DNSSearchDomainsDisabled;

        SimpleOutput.WriteLine(enableMessage);

    }

    private static async Task AddHandler(string[] searchDomains)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedSearchDomain = false;
        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            if (config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                output.WithInformation(CommandLineStrings.DNSSearchDomainAlreadyDefined, searchDomain);
                continue;
            }

            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            output.With(searchDomain);

            hasAddedSearchDomain = true;
        }

        if (hasAddedSearchDomain)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.DNSSearchDomainAdded
                : CommandLineStrings.DNSSearchDomainsAdded;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.SearchDomainsEnabled)
                output.WithWarning(CommandLineStrings.DNSSearchDomainsCurrentlyDisabled);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(string[] searchDomains)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedSearchDomain = false;
        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            if (!config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                output.WithWarning(CommandLineStrings.DNSSearchDomainNotFound, searchDomain);
                continue;
            }

            if (!config.NetworkDNSSettings.SearchDomains.Remove(searchDomain))
            {
                output
                    .WithError(CommandLineStrings.DNSSearchDomainCouldNotBeRemoved, searchDomain)
                    .WriteLine();

                return;
            }

            output.With(searchDomain);

            hasRemovedSearchDomain = true;
        }

        if (hasRemovedSearchDomain)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.DNSSearchDomainRemoved
                : CommandLineStrings.DNSSearchDomainsRemoved;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.SearchDomainsEnabled &&
                config.NetworkDNSSettings.SearchDomains.Count > 0)
            {
                output.WithWarning(CommandLineStrings.DNSSearchDomainsCurrentlyDisabled);
            }
        }

        output.WriteLine();
    }

    private static async Task SetHandler(string[] searchDomains)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.SearchDomains.Clear();

        var output = new ListOutput();

        foreach (var searchDomain in searchDomains)
        {
            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            output.With(searchDomain);
        }

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings.DNSSearchDomainSet
            : CommandLineStrings.DNSSearchDomainsSet;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.SearchDomainsEnabled)
            output.WithWarning(CommandLineStrings.DNSSearchDomainsCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.DNSSearchDomainsNotConfigured);

            return;
        }

        config.NetworkDNSSettings.SearchDomains.Clear();

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.DNSSearchDomainsCleared);
    }

    private static async Task ModifyHandler(string oldSearchDomain, string newSearchDomain)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var searchDomains = config.NetworkDNSSettings.SearchDomains;

        if (searchDomains.Contains(newSearchDomain))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.DNSSearchDomainAlreadyDefined, newSearchDomain);

            return;
        }

        var index = searchDomains.IndexOf(oldSearchDomain);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings.DNSSearchDomainNotFound, oldSearchDomain);

            return;
        }

        searchDomains[index] = newSearchDomain;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (!config.NetworkDNSSettings.SearchDomainsEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings.DNSSearchDomainsCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings.DNSSearchDomainModified, oldSearchDomain, newSearchDomain);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings.DNSSearchDomainsNotConfigured);

            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings.DNSSearchDomains, config.NetworkDNSSettings.SearchDomainsEnabled)
            .WithEach(config.NetworkDNSSettings.SearchDomains.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
