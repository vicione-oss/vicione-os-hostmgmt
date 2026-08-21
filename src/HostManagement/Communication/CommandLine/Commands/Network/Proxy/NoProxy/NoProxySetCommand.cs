using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="noProxyEntriesArgument">The no-proxy entries argument.</param>
    public NoProxySetCommand(Argument<string[]> noProxyEntriesArgument)
        : base(CliStrings.Common.SetCommandName, CliStrings.Network.Proxy.NoProxy.SetCommandDescription)
    {
        Add(noProxyEntriesArgument);

        SetAction((parseResult, cancellationToken)
            => SetHandler(parseResult.GetRequiredValue(noProxyEntriesArgument), cancellationToken));
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
            ? CliStrings.Network.Proxy.NoProxy.Set
            : CliStrings.Network.Proxy.NoProxy.SetMultiple;

        output.WithTitle(title);

        if (!config.NetworkProxySettings.NoProxy.Enabled)
            output.WithWarning(CliStrings.Network.Proxy.NoProxy.CurrentlyDisabled);

        output.WriteLine();
    }
}
