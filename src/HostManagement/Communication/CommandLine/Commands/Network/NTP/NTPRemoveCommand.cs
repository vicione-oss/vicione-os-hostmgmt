using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP remove command.
/// </summary>
internal sealed class NTPRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPRemoveCommand"/>.
    /// </summary>
    /// <param name="ntpServersArgument">The NTP servers argument.</param>
    public NTPRemoveCommand(Argument<string[]> ntpServersArgument)
        : base(CliStrings.Common.RemoveCommandName, CliStrings.Network.NTP.RemoveCommandDescription)
    {
        Add(ntpServersArgument);

        SetAction((parseResult, cancellationToken)
            => RemoveHandler(parseResult.GetRequiredValue(ntpServersArgument), cancellationToken));
    }

    private static async Task RemoveHandler(string[] servers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasRemovedServer = false;
        var output = new ListOutput();

        foreach (var server in servers)
        {
            if (!config.NetworkNTPSettings.Servers.Contains(server))
            {
                output.WithWarning(CliStrings.Network.NTP.NotFound, server);
                continue;
            }

            if (!config.NetworkNTPSettings.Servers.Remove(server))
            {
                output.WithError(CliStrings.Network.NTP.CouldNotBeRemoved, server).WriteLine();
                return;
            }

            output.With(server);

            hasRemovedServer = true;
        }

        if (hasRemovedServer)
        {
            if (config.NetworkNTPSettings.Enabled)
            {
                if (config.NetworkNTPSettings.Servers.Count == 0)
                {
                    config.NetworkNTPSettings.Enabled = false;

                    output.WithInformation(CliStrings.Network.NTP.Disabled);
                }
            }
            else
            {
                if (config.NetworkNTPSettings.Servers.Count > 0)
                {
                    output.WithWarning(CliStrings.Network.NTP.CurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CliStrings.Network.NTP.Removed
                : CliStrings.Network.NTP.RemovedMultiple;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}
