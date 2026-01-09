using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
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
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyListEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.DoNotProxyListAlreadyEnabled
                : CommandLineStrings.DoNotProxyListAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        config.NetworkProxySettings.DoNotProxyListEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (enable &&
            config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.DoNotProxyListNotConfigured);
        }

        var enableMessage = enable
            ? CommandLineStrings.DoNotProxyListEnabled
            : CommandLineStrings.DoNotProxyListDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task AddHandler(string[] noProxyItems)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedNoProxyItems = false;
        var output = new ListOutput();

        foreach (var noProxyItem in noProxyItems)
        {
            if (config.NetworkProxySettings.DoNotProxyList.Contains(noProxyItem, StringComparer.OrdinalIgnoreCase))
            {
                output.WithInformation(CommandLineStrings.DoNotProxyItemAlreadyDefined, noProxyItem);
                continue;
            }

            config.NetworkProxySettings.DoNotProxyList.Add(noProxyItem);
            output.With(noProxyItem);

            hasAddedNoProxyItems = true;
        }

        if (hasAddedNoProxyItems)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.DoNotProxyItemAdded
                : CommandLineStrings.DoNotProxyItemsAdded;

            output.WithTitle(title);

            if (!config.NetworkProxySettings.DoNotProxyListEnabled)
                output.WithWarning(CommandLineStrings.DoNotProxyListCurrentlyDisabled);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(string[] itemsOptionValue)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedNoProxyItems = false;
        var output = new ListOutput();

        foreach (var noProxyItem in itemsOptionValue)
        {
            if (!config.NetworkProxySettings.DoNotProxyList.Contains(noProxyItem, StringComparer.OrdinalIgnoreCase))
            {
                output.WithWarning(CommandLineStrings.DoNotProxyItemNotFound, noProxyItem);
                continue;
            }

            if (!config.NetworkProxySettings.DoNotProxyList.Remove(noProxyItem))
            {
                output
                    .WithError(CommandLineStrings.DoNotProxyItemCouldNotBeRemoved, noProxyItem)
                    .WriteLine();

                return;
            }

            output.With(noProxyItem);

            hasRemovedNoProxyItems = true;
        }

        if (hasRemovedNoProxyItems)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.DoNotProxyItemRemoved
                : CommandLineStrings.DoNotProxyItemsRemoved;

            output.WithTitle(title);

            if (!config.NetworkProxySettings.DoNotProxyListEnabled &&
                config.NetworkProxySettings.DoNotProxyList.Count > 0)
            {
                output.WithWarning(CommandLineStrings.DoNotProxyListCurrentlyDisabled);
            }
        }

        output.WriteLine();
    }

    private static async Task SetHandler(string[] noProxyItems)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkProxySettings.DoNotProxyList.Clear();

        var output = new ListOutput();

        foreach (var noProxyItem in noProxyItems)
        {
            config.NetworkProxySettings.DoNotProxyList.Add(noProxyItem);
            output.With(noProxyItem);
        }

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings.DoNotProxyItemSet
            : CommandLineStrings.DoNotProxyItemsSet;

        output.WithTitle(title);

        if (!config.NetworkProxySettings.DoNotProxyListEnabled)
            output.WithWarning(CommandLineStrings.DoNotProxyListCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.DoNotProxyListNotConfigured);

            return;
        }

        config.NetworkProxySettings.DoNotProxyList.Clear();

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.DoNotProxyListCleared);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings.DoNotProxyListNotConfigured);

            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings.DoNotProxyList, config.NetworkProxySettings.DoNotProxyListEnabled)
            .WithEach(config.NetworkProxySettings.DoNotProxyList.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
