using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP status command.
/// </summary>
internal sealed class NTPStatusCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="NTPStatusCommand"/>.
    /// </summary>
    public NTPStatusCommand()
        : base(CliStrings.Common.StatusCommandName, CliStrings.Network.NTP.StatusCommandDescription)
        => SetAction((parseResult, cancellationToken) => StatusHandler(cancellationToken));

    private static async Task StatusHandler(CancellationToken cancellationToken)
    {
        if (await NamedPipeRequestHelper.GetNTPSyncStatus(cancellationToken) is not NTPSyncStatus syncStatus)
            return;

        new KeyValueOutput()
            .With(new(CliStrings.Network.NTP.ServerAddress, syncStatus.ServerAddress))
            .With(new(CliStrings.Network.NTP.ServerName, syncStatus.ServerName))
            .With(new(CliStrings.Network.NTP.Synchronized, syncStatus.Synchronized))
            .With(new(
                CliStrings.Network.NTP.Stratum,
                $"{syncStatus.Stratum} ({CliStrings.Network.NTP.Quality}: {syncStatus.StratumQuality})"))
            .With(new(
                CliStrings.Network.NTP.Offset,
                $"{syncStatus.Offset} ms ({CliStrings.Network.NTP.Quality}: {syncStatus.OffsetQuality})"))
            .With(new(
                CliStrings.Network.NTP.Jitter,
                $"{syncStatus.Jitter} ms ({CliStrings.Network.NTP.Quality}: {syncStatus.JitterQuality})"))
            .With(new(CliStrings.Network.NTP.Message, syncStatus.Message))
            .WriteLine();
    }
}
