using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
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
    public NTPRemoveCommand(Argument<string[]> ntpServersArgument) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_NTP.NTPRemoveCommandDescription)
    {
        Add(ntpServersArgument);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(ntpServersArgument),
                cancellationToken);
        });
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
                output.WithWarning(CommandLineStrings_NTP.ServerNotFound, server);
                continue;
            }

            if (!config.NetworkNTPSettings.Servers.Remove(server))
            {
                output
                    .WithError(CommandLineStrings_NTP.ServerCouldNotBeRemoved, server)
                    .WriteLine();

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

                    output.WithInformation(CommandLineStrings_NTP.ServersDisabled);
                }
            }
            else
            {
                if (config.NetworkNTPSettings.Servers.Count > 0)
                {
                    output.WithWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_NTP.ServerRemoved
                : CommandLineStrings_NTP.ServersRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}
