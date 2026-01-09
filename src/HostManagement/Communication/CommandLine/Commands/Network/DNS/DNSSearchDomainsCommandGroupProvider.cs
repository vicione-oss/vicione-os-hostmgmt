using System.CommandLine;
using System.Text;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
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
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.SearchDomainsEnabled = enable;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
            ? CommandLineStrings.DNSSearchDomainsEnabled
            : CommandLineStrings.DNSSearchDomainsDisabled);
    }

    private static async Task AddHandler(string[] searchDomainItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        foreach (var searchDomain in searchDomainItemsOptionValue)
        {
            if (config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                ConsoleEx.WriteLineWarning(
                    CommandLineStrings.DNSSearchDomainAlreadyDefined,
                    searchDomain.ToString());

                continue;
            }

            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            builder.Append(searchDomain.ToString() + ", ");
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        if (builder.Length > 0)
        {
            Console.WriteLine(
                CommandLineStrings.DNSSearchDomainsAdded,
                builder.ToString().TrimEnd().TrimEnd(','));
        }
    }

    private static async Task RemoveHandler(string[] searchDomainItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        foreach (var searchDomain in searchDomainItemsOptionValue)
        {
            if (!config.NetworkDNSSettings.SearchDomains.Contains(searchDomain))
            {
                ConsoleEx.WriteLineWarning(
                    CommandLineStrings.DNSSearchDomainNotFound,
                    searchDomain.ToString());

                continue;
            }

            if (!config.NetworkDNSSettings.SearchDomains.Remove(searchDomain))
            {
                ConsoleEx.WriteLineError(
                    CommandLineStrings.DNSSearchDomainCouldNotBeRemoved,
                    searchDomain.ToString());

                return;
            }

            builder.Append(searchDomain.ToString() + ", ");
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        if (builder.Length > 0)
        {
            Console.WriteLine(
            CommandLineStrings.DNSSearchDomainsRemoved,
            builder.ToString().TrimEnd().TrimEnd(','));
        }
    }

    private static async Task SetHandler(string[] searchDomainItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.SearchDomains.Clear();

        var builder = new StringBuilder();

        foreach (var searchDomain in searchDomainItemsOptionValue)
        {
            config.NetworkDNSSettings.SearchDomains.Add(searchDomain);
            builder.Append(searchDomain.ToString() + ", ");
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        if (builder.Length > 0)
        {
            Console.WriteLine(
                CommandLineStrings.DNSSearchDomainsSet,
                builder.ToString().TrimEnd().TrimEnd(','));
        }
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.SearchDomains.Clear();

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.DNSSearchDomainsCleared);
    }

    private static async Task ModifyHandler(string oldSearchDomainOptionValue, string newSearchDomainOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var searchDomains = config.NetworkDNSSettings.SearchDomains;

        if (searchDomains.Contains(newSearchDomainOptionValue))
        {
            ConsoleEx.WriteLineWarning(CommandLineStrings.DNSSearchDomainAlreadyDefined, newSearchDomainOptionValue);
            return;
        }

        var index = searchDomains.IndexOf(oldSearchDomainOptionValue);

        if (index == -1)
        {
            ConsoleEx.WriteLineError(CommandLineStrings.DNSSearchDomainNotFound, oldSearchDomainOptionValue);
            return;
        }

        searchDomains[index] = newSearchDomainOptionValue;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.DNSSearchDomainModified, oldSearchDomainOptionValue, newSearchDomainOptionValue);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            Console.WriteLine(CommandLineStrings.DNSSearchDomainsNotDefined);
            return;
        }

        var builder = new StringBuilder();

        builder.AppendLine($"Search domains ({(config.NetworkDNSSettings.SearchDomainsEnabled ? "enabled" : "disabled")})");

        foreach (var searchDomain in config.NetworkDNSSettings.SearchDomains)
            builder.AppendLine($"  {searchDomain}");

        Console.WriteLine(builder.ToString().TrimEnd('\r', '\n'));
    }
}
