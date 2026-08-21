using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public NTPEnableCommand()
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.NTP.EnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.NTP.AlreadyEnabled);
            return;
        }

        if (config.NetworkNTPSettings.Servers.Count == 0)
        {
            SimpleOutput.WriteLineError(CliStrings.Network.NTP.NotConfigured);
            return;
        }

        config.NetworkNTPSettings.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.NTP.Enabled);
    }
}
