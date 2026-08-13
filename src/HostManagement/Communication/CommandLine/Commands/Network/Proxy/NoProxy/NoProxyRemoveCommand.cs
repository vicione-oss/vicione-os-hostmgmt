using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy remove command.
/// </summary>
internal sealed class NoProxyRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyRemoveCommand"/>.
    /// </summary>
    /// <param name="noProxyEntriesArgument">The no-proxy entries argument.</param>
    public NoProxyRemoveCommand(Argument<string[]> noProxyEntriesArgument) : base(
        CliStrings.Common.RemoveCommandName,
        CommandLineStrings_Proxy.NoProxyListRemoveCommandDescription)
    {
        Add(noProxyEntriesArgument);

        SetAction((parseResult, cancellationToken)
            => RemoveHandler(parseResult.GetRequiredValue(noProxyEntriesArgument), cancellationToken));
    }

    private static async Task RemoveHandler(string[] entries, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasRemovedEntry = false;
        var output = new ListOutput();

        foreach (var entry in entries)
        {
            if (!config.NetworkProxySettings.NoProxy.Entries.Contains(entry, StringComparer.OrdinalIgnoreCase))
            {
                output.WithWarning(CommandLineStrings_Proxy.NoProxyItemNotFound, entry);
                continue;
            }

            if (!config.NetworkProxySettings.NoProxy.Entries.Remove(entry))
            {
                output
                    .WithError(CommandLineStrings_Proxy.NoProxyItemCouldNotBeRemoved, entry)
                    .WriteLine();

                return;
            }

            output.With(entry);

            hasRemovedEntry = true;
        }

        if (hasRemovedEntry)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_Proxy.NoProxyItemRemoved
                : CommandLineStrings_Proxy.NoProxyItemsRemoved;

            output.WithTitle(title);

            if (!config.NetworkProxySettings.NoProxy.Enabled &&
                config.NetworkProxySettings.NoProxy.Entries.Count > 0)
            {
                output.WithWarning(CommandLineStrings_Proxy.NoProxyListCurrentlyDisabled);
            }
        }

        output.WriteLine();
    }
}
