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
    /// <param name="ntpServersOption">The NTP servers option.</param>
    public NTPRemoveCommand(Option<string[]> ntpServersOption) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_NTP.NTPRemoveCommandDescription)
    {
        Add(ntpServersOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(ntpServersOption),
                cancellationToken);
        });
    }

    private static async Task RemoveHandler(string[] ntpServers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasRemovedNtpServer = false;
        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            if (!config.NetworkNTPSettings.NTPServers.Contains(ntpServer))
            {
                output.WithWarning(CommandLineStrings_NTP.ServerNotFound, ntpServer);
                continue;
            }

            if (!config.NetworkNTPSettings.NTPServers.Remove(ntpServer))
            {
                output
                    .WithError(CommandLineStrings_NTP.ServerCouldNotBeRemoved, ntpServer)
                    .WriteLine();

                return;
            }

            output.With(ntpServer);

            hasRemovedNtpServer = true;
        }

        if (hasRemovedNtpServer)
        {
            if (config.NetworkNTPSettings.NTPServersEnabled)
            {
                if (config.NetworkNTPSettings.NTPServers.Count == 0)
                {
                    config.NetworkNTPSettings.NTPServersEnabled = false;

                    output.WithInformation(CommandLineStrings_NTP.ServersDisabled);
                }
            }
            else
            {
                if (config.NetworkNTPSettings.NTPServers.Count > 0)
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
