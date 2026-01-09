using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP add command.
/// </summary>
internal sealed class NTPAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPAddCommand"/>.
    /// </summary>
    /// <param name="ntpServersOption">The NTP servers option.</param>
    public NTPAddCommand(Option<string[]> ntpServersOption) : base(
        CommandLineStrings_Common.AddCommandName,
        CommandLineStrings_NTP.NTPAddCommandDescription)
    {
        Add(ntpServersOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return AddHandler(
                parseResult.GetRequiredValue(ntpServersOption),
                cancellationToken);
        });
    }

    private static async Task AddHandler(string[] ntpServers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasAddedNtpServer = false;
        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            if (config.NetworkNTPSettings.NTPServers.Contains(ntpServer))
            {
                output.WithInformation(CommandLineStrings_NTP.ServerAlreadyDefined, ntpServer);
                continue;
            }

            config.NetworkNTPSettings.NTPServers.Add(ntpServer);
            output.With(ntpServer);

            hasAddedNtpServer = true;
        }

        if (hasAddedNtpServer)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            if (!config.NetworkNTPSettings.NTPServersEnabled)
                output.WithWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);

            var title = output.ItemCount == 1
                ? CommandLineStrings_NTP.ServerAdded
                : CommandLineStrings_NTP.ServersAdded;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}
