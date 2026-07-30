using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP enable command.
/// </summary>
internal sealed class NTPEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPEnableCommand"/>.
    /// </summary>
    public NTPEnableCommand() : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_NTP.NTPEnableCommandDescription)
            => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NTP.ServersAlreadyEnabled);
            return;
        }

        if (config.NetworkNTPSettings.Servers.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_NTP.ServersNotConfigured);
            return;
        }

        config.NetworkNTPSettings.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_NTP.ServersEnabled);
    }
}
