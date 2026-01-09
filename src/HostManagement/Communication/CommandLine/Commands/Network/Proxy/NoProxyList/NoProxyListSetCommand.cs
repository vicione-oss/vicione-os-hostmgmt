using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list set command.
/// </summary>
internal sealed class NoProxyListSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListSetCommand"/>.
    /// </summary>
    /// <param name="noProxyItemsOption">The no-proxy items option.</param>
    public NoProxyListSetCommand(Option<string[]> noProxyItemsOption) : base(
        CommandLineStrings_Common.SetCommandName,
        CommandLineStrings_Proxy.NoProxyListSetCommandDescription)
    {
        Add(noProxyItemsOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return SetHandler(
                parseResult.GetRequiredValue(noProxyItemsOption),
                cancellationToken);
        });
    }

    private static async Task SetHandler(string[] noProxyItems, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkProxySettings.DoNotProxyList.Clear();

        var output = new ListOutput();

        foreach (var noProxyItem in noProxyItems)
        {
            config.NetworkProxySettings.DoNotProxyList.Add(noProxyItem);
            output.With(noProxyItem);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_Proxy.NoProxyItemSet
            : CommandLineStrings_Proxy.NoProxyItemsSet;

        output.WithTitle(title);

        if (!config.NetworkProxySettings.DoNotProxyListEnabled)
            output.WithWarning(CommandLineStrings_Proxy.NoProxyListCurrentlyDisabled);

        output.WriteLine();
    }
}
