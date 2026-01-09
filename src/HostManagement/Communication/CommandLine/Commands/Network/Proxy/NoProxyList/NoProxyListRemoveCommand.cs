using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list remove command.
/// </summary>
internal sealed class NoProxyListRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListRemoveCommand"/>.
    /// </summary>
    /// <param name="noProxyItemsOption">The no-proxy items option.</param>
    public NoProxyListRemoveCommand(Option<string[]> noProxyItemsOption) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_Proxy.NoProxyListRemoveCommandDescription)
    {
        Add(noProxyItemsOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(noProxyItemsOption),
                cancellationToken);
        });
    }

    private static async Task RemoveHandler(string[] noProxyItems, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasRemovedNoProxyItems = false;
        var output = new ListOutput();

        foreach (var noProxyItem in noProxyItems)
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
            if (!await SetConfiguration(config, cancellationToken))
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
}
