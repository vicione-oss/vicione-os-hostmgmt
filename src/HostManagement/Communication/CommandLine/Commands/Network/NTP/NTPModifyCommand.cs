using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP modify command.
/// </summary>
internal sealed class NTPModifyCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NTPModifyCommand"/>.
    /// </summary>
    /// <param name="oldNTPServerOption">The old NTP server option.</param>
    /// <param name="newNTPServerOption">The new NTP server option.</param>
    public NTPModifyCommand(Option<string> oldNTPServerOption, Option<string> newNTPServerOption) : base(
        CommandLineStrings_Common.ModifyCommandName,
        CommandLineStrings_NTP.NTPModifyCommandDescription)
    {
        Add(oldNTPServerOption);
        Add(newNTPServerOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return ModifyHandler(
                parseResult.GetRequiredValue(oldNTPServerOption),
                parseResult.GetRequiredValue(newNTPServerOption),
                cancellationToken);
        });
    }

    private static async Task ModifyHandler(string oldServer, string newServer, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var servers = config.NetworkNTPSettings.Servers;

        if (servers.Contains(newServer))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NTP.ServerAlreadyDefined, newServer);
            return;
        }

        var index = servers.IndexOf(oldServer);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_NTP.ServerNotFound, oldServer);
            return;
        }

        servers[index] = newServer;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (!config.NetworkNTPSettings.Enabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_NTP.ServerModified, oldServer, newServer);
    }
}
