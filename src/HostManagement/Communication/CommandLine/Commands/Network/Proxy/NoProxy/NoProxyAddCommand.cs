using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy list command.
/// </summary>
internal sealed class NoProxyAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyAddCommand"/>.
    /// </summary>
    /// <param name="noProxyItemsOption">The no-proxy items option.</param>
    public NoProxyAddCommand(Option<string[]> noProxyItemsOption) : base(
        CommandLineStrings_Common.AddCommandName,
        CommandLineStrings_Proxy.NoProxyListAddCommandDescription)
    {
        Add(noProxyItemsOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return AddHandler(
                parseResult.GetRequiredValue(noProxyItemsOption),
                cancellationToken);
        });
    }

    private static async Task AddHandler(string[] entries, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasAddedEntry = false;
        var output = new ListOutput();

        foreach (var entry in entries)
        {
            if (config.NetworkProxySettings.NoProxy.Entries.Contains(entry, StringComparer.OrdinalIgnoreCase))
            {
                output.WithInformation(CommandLineStrings_Proxy.NoProxyItemAlreadyDefined, entry);
                continue;
            }

            config.NetworkProxySettings.NoProxy.Entries.Add(entry);
            output.With(entry);

            hasAddedEntry = true;
        }

        if (hasAddedEntry)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_Proxy.NoProxyItemAdded
                : CommandLineStrings_Proxy.NoProxyItemsAdded;

            output.WithTitle(title);

            if (!config.NetworkProxySettings.NoProxy.Enabled)
                output.WithWarning(CommandLineStrings_Proxy.NoProxyListCurrentlyDisabled);
        }

        output.WriteLine();
    }
}
