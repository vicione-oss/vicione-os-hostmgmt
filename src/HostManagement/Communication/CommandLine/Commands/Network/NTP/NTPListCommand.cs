using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP list command.
/// </summary>
internal sealed class NTPListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPListCommand"/>.
    /// </summary>
    public NTPListCommand() : base(
        CommandLineStrings_Common.ListCommandName,
        CommandLineStrings_NTP.NTPListCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));
    }

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.NTPServers.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings_NTP.ServersNotConfigured);
        }
        else
        {
            new ListOutput()
                .WithTitle(CommandLineStrings_NTP.Servers, config.NetworkNTPSettings.NTPServersEnabled)
                .WithEach(config.NetworkNTPSettings.NTPServers.Select(CliOutputValue.Create))
                .WriteLine();
        }

        var ntpFallbackServers = await NamedPipeRequestHelper.GetNTPFallbackInformation();

        new ListOutput()
            .WithTitle(CommandLineStrings_NTP.FallbackServers)
            .WithEach(ntpFallbackServers.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
