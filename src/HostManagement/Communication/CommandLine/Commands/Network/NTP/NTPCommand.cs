using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP command.
/// </summary>
internal sealed class NTPCommand : ConfigCommandBase
{
    private static readonly Option<string> s_oldNTPServerOption = new(["-o", "--old-ntp-server"])
    {
        Description = "The old NTP server.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string> s_newNTPServerOption = new(["-n", "--new-ntp-server"])
    {
        Description = "The new NTP server.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string[]> s_ntpServerItemsOption = CommonOptions.s_itemsOption;

    /// <summary>
    /// Initializes a new <see cref="NTPCommand"/>.
    /// </summary>
    public NTPCommand() : base(
        CommandLineStrings_NTP.NTPCommandName,
        CommandLineStrings_NTP.NTPCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_NTP.NTPEnableCommandDescription,
            () => EnableDisableHandler(enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_NTP.NTPDisableCommandDescription,
            () => EnableDisableHandler(enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_NTP.NTPListCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_Common.AddCommandName,
            CommandLineStrings_NTP.NTPAddCommandDescription,
            s_ntpServerItemsOption,
            AddHandler);

        Add(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_NTP.NTPRemoveCommandDescription,
            s_ntpServerItemsOption,
            RemoveHandler);

        Add(
            CommandLineStrings_Common.SetCommandName,
            CommandLineStrings_NTP.NTPSetCommandDescription,
            s_ntpServerItemsOption,
            SetHandler);

        Add(
            CommandLineStrings_Common.ClearCommandName,
            CommandLineStrings_NTP.NTPClearCommandDescription,
            ClearHandler);

        Add(
            CommandLineStrings_Common.ModifyCommandName,
            CommandLineStrings_NTP.NTPModifyCommandDescription,
            s_oldNTPServerOption,
            s_newNTPServerOption,
            ModifyHandler);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.NTPServersEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_NTP.ServersAlreadyEnabled
                : CommandLineStrings_NTP.ServersAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        if (enable &&
            config.NetworkNTPSettings.NTPServers.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_NTP.ServersNotConfigured);
        }

        config.NetworkNTPSettings.NTPServersEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_NTP.ServersEnabled
            : CommandLineStrings_NTP.ServersDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task AddHandler(string[] ntpServers)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedNtpServer = false;
        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            if (config.NetworkNTPSettings.NTPServers.Contains(ntpServer))
            {
                output.WithInformation(CommandLineStrings_NTP.ServerAlreadyDefined, ntpServer);
                continue;
            }

            config.NetworkNTPSettings.NTPServers.Add(ntpServer);
            output.With(ntpServer);

            hasAddedNtpServer = true;
        }

        if (hasAddedNtpServer)
        {
            if (!await SetConfiguration(config))
                return;

            if (!config.NetworkNTPSettings.NTPServersEnabled)
                output.WithWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);

            var title = output.ItemCount == 1
                ? CommandLineStrings_NTP.ServerAdded
                : CommandLineStrings_NTP.ServersAdded;

            output.WithTitle(title);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(string[] ntpServers)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedNtpServer = false;
        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            if (!config.NetworkNTPSettings.NTPServers.Contains(ntpServer))
            {
                output.WithWarning(CommandLineStrings_NTP.ServerNotFound, ntpServer);
                continue;
            }

            if (!config.NetworkNTPSettings.NTPServers.Remove(ntpServer))
            {
                output
                    .WithError(CommandLineStrings_NTP.ServerCouldNotBeRemoved, ntpServer)
                    .WriteLine();

                return;
            }

            output.With(ntpServer);

            hasRemovedNtpServer = true;
        }

        if (hasRemovedNtpServer)
        {
            if (config.NetworkNTPSettings.NTPServersEnabled)
            {
                if (config.NetworkNTPSettings.NTPServers.Count == 0)
                {
                    config.NetworkNTPSettings.NTPServersEnabled = false;

                    output.WithInformation(CommandLineStrings_NTP.ServersDisabled);
                }
            }
            else
            {
                if (config.NetworkNTPSettings.NTPServers.Count > 0)
                {
                    output.WithWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_NTP.ServerRemoved
                : CommandLineStrings_NTP.ServersRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }

    private static async Task SetHandler(string[] ntpServers)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkNTPSettings.NTPServers.Clear();

        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            config.NetworkNTPSettings.NTPServers.Add(ntpServer);
            output.With(ntpServer);
        }

        if (!await SetConfiguration(config))
            return;

        if (!config.NetworkNTPSettings.NTPServersEnabled)
            output.WithWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);

        var title = output.ItemCount == 1
            ? CommandLineStrings_NTP.ServerSet
            : CommandLineStrings_NTP.ServersSet;

        output.WithTitle(title).WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.NTPServers.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NTP.ServersNotConfigured);
            return;
        }

        config.NetworkNTPSettings.NTPServers.Clear();
        config.NetworkNTPSettings.NTPServersEnabled = false;

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLineInformation(CommandLineStrings_NTP.ServersDisabled);
        SimpleOutput.WriteLine(CommandLineStrings_NTP.ServersCleared);
    }

    private static async Task ModifyHandler(string oldNTPServer, string newNTPServer)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
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

        if (!await SetConfiguration(config))
            return;

        if (!config.NetworkNTPSettings.NTPServersEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_NTP.ServersCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_NTP.ServerModified, oldNTPServer, newNTPServer);
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
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
