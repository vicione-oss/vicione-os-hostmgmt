using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="ntpServersArgument">The NTP servers argument.</param>
    public NTPAddCommand(Argument<string[]> ntpServersArgument)
        : base(CliStrings.Common.AddCommandName, CliStrings.Network.NTP.AddCommandDescription)
    {
        Add(ntpServersArgument);

        SetAction((parseResult, cancellationToken)
            => AddHandler(parseResult.GetRequiredValue(ntpServersArgument), cancellationToken));
    }

    private static async Task AddHandler(string[] servers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasAddedServer = false;
        var output = new ListOutput();

        foreach (var server in servers)
        {
            if (config.NetworkNTPSettings.Servers.Contains(server))
            {
                output.WithInformation(CliStrings.Network.NTP.AlreadyDefined, server);
                continue;
            }

            config.NetworkNTPSettings.Servers.Add(server);
            output.With(server);

            hasAddedServer = true;
        }

        if (hasAddedServer)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            if (!config.NetworkNTPSettings.Enabled)
                output.WithWarning(CliStrings.Network.NTP.CurrentlyDisabled);

            var title = output.ItemCount == 1
                ? CliStrings.Network.NTP.Added
                : CliStrings.Network.NTP.AddedMultiple;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}
