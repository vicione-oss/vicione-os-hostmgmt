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

    private static async Task ModifyHandler(
        string oldNTPServer,
        string newNTPServer,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var ntpServers = config.NetworkNTPSettings.NTPServers;

        if (ntpServers.Contains(newNTPServer))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NTP.ServerAlreadyDefined, newNTPServer);
            return;
        }

        var index = ntpServers.IndexOf(oldNTPServer);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_NTP.ServerNotFound, oldNTPServer);
            return;
        }

        ntpServers[index] = newNTPServer;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (!config.NetworkNTPSettings.NTPServersEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_NTP.ServerModified, oldNTPServer, newNTPServer);
    }
}
