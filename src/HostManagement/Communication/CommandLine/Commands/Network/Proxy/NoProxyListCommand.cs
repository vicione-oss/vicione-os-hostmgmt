using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the no-proxy list command.
/// </summary>
internal sealed class NoProxyListCommand : ConfigCommandBase
{
    private static readonly Option<string[]> s_itemsOption = CommonOptions.s_itemsOption;

    /// <summary>
    /// Initializes a new <see cref="NoProxyListCommand"/>.
    /// </summary>
    public NoProxyListCommand() : base(
        CommandLineStrings_Proxy.NoProxyListCommandName,
        CommandLineStrings_Proxy.NoProxyListCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_Proxy.NoProxyListEnableCommandDescription,
            () => EnableDisableHandler(enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_Proxy.NoProxyListDisableCommandDescription,
            () => EnableDisableHandler(enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_Proxy.NoProxyListListCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_Common.AddCommandName,
            CommandLineStrings_Proxy.NoProxyListAddCommandDescription,
            s_itemsOption,
            AddHandler);
        
        Add(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_Proxy.NoProxyListRemoveCommandDescription,
            s_itemsOption,
            RemoveHandler);

        Add(
            CommandLineStrings_Common.SetCommandName,
            CommandLineStrings_Proxy.NoProxyListSetCommandDescription,
            s_itemsOption,
            SetHandler);

        Add(
            CommandLineStrings_Common.ClearCommandName,
            CommandLineStrings_Proxy.NoProxyListClearCommandDescription,
            ClearHandler);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyListEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_Proxy.NoProxyListAlreadyEnabled
                : CommandLineStrings_Proxy.NoProxyListAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        if (enable &&
            config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        config.NetworkProxySettings.DoNotProxyListEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_Proxy.NoProxyListEnabled
            : CommandLineStrings_Proxy.NoProxyListDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task AddHandler(string[] noProxyItems)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedNoProxyItems = false;
        var output = new ListOutput();

        foreach (var noProxyItem in noProxyItems)
        {
            if (config.NetworkProxySettings.DoNotProxyList.Contains(noProxyItem, StringComparer.OrdinalIgnoreCase))
            {
                output.WithInformation(CommandLineStrings_Proxy.NoProxyItemAlreadyDefined, noProxyItem);
                continue;
            }

            config.NetworkProxySettings.DoNotProxyList.Add(noProxyItem);
            output.With(noProxyItem);

            hasAddedNoProxyItems = true;
        }

        if (hasAddedNoProxyItems)
        {
            if (!await SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_Proxy.NoProxyItemAdded
                : CommandLineStrings_Proxy.NoProxyItemsAdded;

            output.WithTitle(title);

            if (!config.NetworkProxySettings.DoNotProxyListEnabled)
                output.WithWarning(CommandLineStrings_Proxy.NoProxyListCurrentlyDisabled);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(string[] itemsOptionValue)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedNoProxyItems = false;
        var output = new ListOutput();

        foreach (var noProxyItem in itemsOptionValue)
        {
            if (!config.NetworkProxySettings.DoNotProxyList.Contains(noProxyItem, StringComparer.OrdinalIgnoreCase))
            {
                output.WithWarning(CommandLineStrings_Proxy.NoProxyItemNotFound, noProxyItem);
                continue;
            }

            if (!config.NetworkProxySettings.DoNotProxyList.Remove(noProxyItem))
            {
                output
                    .WithError(CommandLineStrings_Proxy.NoProxyItemCouldNotBeRemoved, noProxyItem)
                    .WriteLine();

                return;
            }

            output.With(noProxyItem);

            hasRemovedNoProxyItems = true;
        }

        if (hasRemovedNoProxyItems)
        {
            if (!await SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_Proxy.NoProxyItemRemoved
                : CommandLineStrings_Proxy.NoProxyItemsRemoved;

            output.WithTitle(title);

            if (!config.NetworkProxySettings.DoNotProxyListEnabled &&
                config.NetworkProxySettings.DoNotProxyList.Count > 0)
            {
                output.WithWarning(CommandLineStrings_Proxy.NoProxyListCurrentlyDisabled);
            }
        }

        output.WriteLine();
    }

    private static async Task SetHandler(string[] noProxyItems)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkProxySettings.DoNotProxyList.Clear();

        var output = new ListOutput();

        foreach (var noProxyItem in noProxyItems)
        {
            config.NetworkProxySettings.DoNotProxyList.Add(noProxyItem);
            output.With(noProxyItem);
        }

        if (!await SetConfiguration(config))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_Proxy.NoProxyItemSet
            : CommandLineStrings_Proxy.NoProxyItemsSet;

        output.WithTitle(title);

        if (!config.NetworkProxySettings.DoNotProxyListEnabled)
            output.WithWarning(CommandLineStrings_Proxy.NoProxyListCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListNotConfigured);

            return;
        }

        config.NetworkProxySettings.DoNotProxyList.Clear();

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.NoProxyListCleared);
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings_Proxy.NoProxyList, config.NetworkProxySettings.DoNotProxyListEnabled)
            .WithEach(config.NetworkProxySettings.DoNotProxyList.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
