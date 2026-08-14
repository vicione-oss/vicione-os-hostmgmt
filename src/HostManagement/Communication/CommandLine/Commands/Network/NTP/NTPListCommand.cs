using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public NTPListCommand()
        : base(CliStrings.Common.ListCommandName, CliStrings.Network.NTP.ListCommandDescription)
        => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.Servers.Count == 0)
        {
            SimpleOutput.WriteLine(CliStrings.Network.NTP.NotConfigured);
        }
        else
        {
            new ListOutput()
                .WithTitle(CliStrings.Network.NTP.Servers, config.NetworkNTPSettings.Enabled)
                .WithEach(config.NetworkNTPSettings.Servers.Select(CliOutputValue.Create))
                .WriteLine();
        }

        var ntpFallbackServers = await NamedPipeRequestHelper.GetNTPFallbackInformation(cancellationToken);

        new ListOutput()
            .WithTitle(CliStrings.Network.NTP.FallbackServers)
            .WithEach(ntpFallbackServers.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
