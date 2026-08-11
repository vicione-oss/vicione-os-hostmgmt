using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP set command.
/// </summary>
internal sealed class NTPSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPSetCommand"/>.
    /// </summary>
    /// <param name="ntpServersArgument">The NTP servers argument.</param>
    public NTPSetCommand(Argument<string[]> ntpServersArgument) : base(
        CommandLineStrings_Common.SetCommandName,
        CommandLineStrings_NTP.NTPSetCommandDescription)
    {
        Add(ntpServersArgument);

        SetAction((parseResult, cancellationToken)
            => SetHandler(parseResult.GetRequiredValue(ntpServersArgument), cancellationToken));
    }

    private static async Task SetHandler(string[] servers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkNTPSettings.Servers.Clear();

        var output = new ListOutput();

        foreach (var server in servers)
        {
            config.NetworkNTPSettings.Servers.Add(server);
            output.With(server);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (!config.NetworkNTPSettings.Enabled)
            output.WithWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);

        var title = output.ItemCount == 1
            ? CommandLineStrings_NTP.ServerSet
            : CommandLineStrings_NTP.ServersSet;

        output.WithTitle(title).WriteLine();
    }
}
