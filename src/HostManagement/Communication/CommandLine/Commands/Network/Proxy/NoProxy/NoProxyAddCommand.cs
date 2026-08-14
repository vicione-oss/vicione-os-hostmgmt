using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="noProxyEntriesArgument">The no-proxy entries argument.</param>
    public NoProxyAddCommand(Argument<string[]> noProxyEntriesArgument)
        : base(CliStrings.Common.AddCommandName, CliStrings.Network.Proxy.NoProxy.AddCommandDescription)
    {
        Add(noProxyEntriesArgument);

        SetAction((parseResult, cancellationToken)
            => AddHandler(parseResult.GetRequiredValue(noProxyEntriesArgument), cancellationToken));
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
                output.WithInformation(CliStrings.Network.Proxy.NoProxy.AlreadyDefined, entry);
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
                ? CliStrings.Network.Proxy.NoProxy.Added
                : CliStrings.Network.Proxy.NoProxy.AddedMultiple;

            output.WithTitle(title);

            if (!config.NetworkProxySettings.NoProxy.Enabled)
                output.WithWarning(CliStrings.Network.Proxy.NoProxy.CurrentlyDisabled);
        }

        output.WriteLine();
    }
}
