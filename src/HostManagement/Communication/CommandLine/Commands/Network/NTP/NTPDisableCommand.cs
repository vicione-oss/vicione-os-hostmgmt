using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP disable command.
/// </summary>
internal sealed class NTPDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPDisableCommand"/>.
    /// </summary>
    public NTPDisableCommand()
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.NTP.DisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkNTPSettings.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.NTP.AlreadyDisabled);
            return;
        }

        config.NetworkNTPSettings.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.NTP.Disabled);
    }
}
