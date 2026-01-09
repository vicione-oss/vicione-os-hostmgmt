using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list add command.
/// </summary>
internal sealed class NoProxyListAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListAddCommand"/>.
    /// </summary>
    /// <param name="noProxyItemsOption">The no-proxy items option.</param>
    public NoProxyListAddCommand(Option<string[]> noProxyItemsOption) : base(
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

    private static async Task AddHandler(string[] noProxyItems, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
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
            if (!await SetConfiguration(config, cancellationToken))
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
}
