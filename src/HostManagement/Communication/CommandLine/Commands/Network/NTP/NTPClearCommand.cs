using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP clear command.
/// </summary>
internal sealed class NTPClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPClearCommand"/>.
    /// </summary>
    public NTPClearCommand() : base(
        CommandLineStrings_Common.ClearCommandName,
        CommandLineStrings_NTP.NTPClearCommandDescription)
            => SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.Servers.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NTP.ServersNotConfigured);
            return;
        }

        config.NetworkNTPSettings.Servers.Clear();
        config.NetworkNTPSettings.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLineInformation(CommandLineStrings_NTP.ServersDisabled);
        SimpleOutput.WriteLine(CommandLineStrings_NTP.ServersCleared);
    }
}
