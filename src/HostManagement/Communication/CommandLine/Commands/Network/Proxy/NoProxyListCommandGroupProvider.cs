using System.CommandLine;
using System.Text;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the <see cref="NoProxyListCommandGroupProvider"/>.
/// </summary>
internal static class NoProxyListCommandGroupProvider
{
    private static readonly Option<string[]> s_itemsOption = CommonOptions.s_itemsOption;

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable the no proxy list.",
        () => EnableDisableHandler(enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable the no proxy list.",
        () => EnableDisableHandler(enable: false));

    private static readonly Command s_addCommand = CommandHelper.ConstructCommand(
        "add",
        "Add the specified items to the no proxy list.",
        s_itemsOption,
        AddHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove the specified items from the no proxy list.",
        s_itemsOption,
        RemoveHandler);

    private static readonly Command s_setCommand = CommandHelper.ConstructCommand(
        "set",
        "Remove existing items from the no proxy list and set new ones.",
        s_itemsOption,
        SetHandler);

    private static readonly Command s_clearCommand = CommandHelper.ConstructCommand(
        "clear",
        "Remove all items from the no proxy list.",
        ClearHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List all items of the no proxy list.",
        ListHandler);

    /// <summary>
    /// Adds the command group to configure the list of ignored URLs by the proxy.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddNoProxyListCommand(this Command parentCommand)
    {
        var noProxyListCommand = new Command(
            "no-proxy-list",
            "Configure the list of ignored URLs by the proxy.");

        noProxyListCommand.AddCommand(s_enableCommand);
        noProxyListCommand.AddCommand(s_disableCommand);
        noProxyListCommand.AddCommand(s_addCommand);
        noProxyListCommand.AddCommand(s_removeCommand);
        noProxyListCommand.AddCommand(s_setCommand);
        noProxyListCommand.AddCommand(s_clearCommand);
        noProxyListCommand.AddCommand(s_listCommand);

        parentCommand.AddCommand(noProxyListCommand);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkProxySettings.DoNotProxyListEnabled = enable;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
            ? CommandLineStrings.NoProxyListEnabled
            : CommandLineStrings.NoProxyListDisabled);
    }

    private static async Task AddHandler(string[] itemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        foreach (var noProxyItem in itemsOptionValue)
        {
            if (!config.NetworkProxySettings.DoNotProxyList.Contains(noProxyItem, StringComparer.OrdinalIgnoreCase)) // TODO OrdinalIgnoreCase?
            {
                config.NetworkProxySettings.DoNotProxyList.Add(noProxyItem);
                builder.Append(noProxyItem + ", ");
            }
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.DoNotProxyItemsAdded,
            builder.ToString().TrimEnd().TrimEnd(','));
    }

    private static async Task RemoveHandler(string[] itemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        foreach (var noProxyItem in itemsOptionValue)
        {
            if (config.NetworkProxySettings.DoNotProxyList.Contains(noProxyItem, StringComparer.OrdinalIgnoreCase))
            {
                config.NetworkProxySettings.DoNotProxyList.Remove(noProxyItem);
                builder.Append(noProxyItem + ", ");
            }
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.DoNotProxyItemsRemoved,
            builder.ToString().TrimEnd().TrimEnd(','));
    }

    private static async Task SetHandler(string[] itemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkProxySettings.DoNotProxyList.Clear();

        var builder = new StringBuilder();

        foreach (var noProxyItem in itemsOptionValue)
        {
            config.NetworkProxySettings.DoNotProxyList.Add(noProxyItem);
            builder.Append(noProxyItem + ", ");
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.DoNotProxyItemsSet,
            builder.ToString().TrimEnd().TrimEnd(','));
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkProxySettings.DoNotProxyList.Clear();

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.DoNotProxyListCleared);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        builder.AppendLine($"No proxy list ({(config.NetworkProxySettings.DoNotProxyListEnabled ? "enabled" : "disabled")})");

        foreach (var noProxyItem in config.NetworkProxySettings.DoNotProxyList)
            builder.AppendLine($"  {noProxyItem}");

        Console.WriteLine(builder.ToString());
    }
}
