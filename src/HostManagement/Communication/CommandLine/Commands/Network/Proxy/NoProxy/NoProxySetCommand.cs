using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy set command.
/// </summary>
internal sealed class NoProxySetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxySetCommand"/>.
    /// </summary>
    /// <param name="noProxyItemsOption">The no-proxy items option.</param>
    public NoProxySetCommand(Option<string[]> noProxyItemsOption) : base(
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

    private static async Task SetHandler(string[] entries, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkProxySettings.NoProxy.Entries.Clear();

        var output = new ListOutput();

        foreach (var entry in entries)
        {
            config.NetworkProxySettings.NoProxy.Entries.Add(entry);
            output.With(entry);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_Proxy.NoProxyItemSet
            : CommandLineStrings_Proxy.NoProxyItemsSet;

        output.WithTitle(title);

        if (!config.NetworkProxySettings.NoProxy.Enabled)
            output.WithWarning(CommandLineStrings_Proxy.NoProxyListCurrentlyDisabled);

        output.WriteLine();
    }
}
