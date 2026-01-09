using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

internal static class NTPCommandGroupProvider
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

    private static readonly Command s_addCommand = CommandHelper.ConstructCommand(
        "add",
        "Add NTP servers.",
        s_ntpServerItemsOption,
        AddHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove NTP servers.",
        s_ntpServerItemsOption,
        RemoveHandler);

    private static readonly Command s_setCommand = CommandHelper.ConstructCommand(
        "set",
        "Remove existing NTP servers and set new ones.",
        s_ntpServerItemsOption,
        SetHandler);

    private static readonly Command s_clearCommand = CommandHelper.ConstructCommand(
        "clear",
        "Remove all NTP servers.",
        ClearHandler);

    private static readonly Command s_modifyCommand = CommandHelper.ConstructCommand(
        "modify",
        "Modify an existing NTP server.",
        s_oldNTPServerOption,
        s_newNTPServerOption,
        ModifyHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List all NTP servers.",
        ListHandler);

    /// <summary>
    /// Adds the command group to configure NTP settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddNTPCommandGroup(this Command parentCommand)
    {
        var ntpCommand = parentCommand.AddCommand(
            "ntp",
            "Configure NTP settings.");

        ntpCommand.AddCommand(s_addCommand);
        ntpCommand.AddCommand(s_removeCommand);
        ntpCommand.AddCommand(s_setCommand);
        ntpCommand.AddCommand(s_clearCommand);
        ntpCommand.AddCommand(s_modifyCommand);
        ntpCommand.AddCommand(s_listCommand);
    }

    private static async Task AddHandler(string[] ntpServers)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedNtpServer = false;
        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            if (config.NetworkNTPSettings.NTPServers.Contains(ntpServer))
            {
                output.WithInformation(CommandLineStrings.NTPServerAlreadyDefined, ntpServer);
                continue;
            }

            config.NetworkNTPSettings.NTPServers.Add(ntpServer);
            output.With(ntpServer);

            hasAddedNtpServer = true;
        }

        if (hasAddedNtpServer)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.NTPServerAdded
                : CommandLineStrings.NTPServersAdded;

            output.WithTitle(title);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(string[] ntpServers)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedNtpServer = false;
        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            if (!config.NetworkNTPSettings.NTPServers.Contains(ntpServer))
            {
                output.WithWarning(CommandLineStrings.NTPServerNotFound, ntpServer);
                continue;
            }

            if (!config.NetworkNTPSettings.NTPServers.Remove(ntpServer))
            {
                output
                    .WithError(CommandLineStrings.NTPServerCouldNotBeRemoved, ntpServer)
                    .WriteLine();

                return;
            }

            output.With(ntpServer);

            hasRemovedNtpServer = true;
        }

        if (hasRemovedNtpServer)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.NTPServerRemoved
                : CommandLineStrings.NTPServersRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }

    private static async Task SetHandler(string[] ntpServers)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkNTPSettings.NTPServers.Clear();

        var output = new ListOutput();

        foreach (var ntpServer in ntpServers)
        {
            config.NetworkNTPSettings.NTPServers.Add(ntpServer);
            output.With(ntpServer);
        }

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings.NTPServerSet
            : CommandLineStrings.NTPServersSet;

        output.WithTitle(title).WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.NTPServers.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.NTPServersNotConfigured);

            return;
        }

        config.NetworkNTPSettings.NTPServers.Clear();

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.NTPServersCleared);
    }

    private static async Task ModifyHandler(string oldNTPServer, string newNTPServer)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var ntpServers = config.NetworkNTPSettings.NTPServers;

        if (ntpServers.Contains(newNTPServer))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.NTPServerAlreadyDefined, newNTPServer);

            return;
        }

        var index = ntpServers.IndexOf(oldNTPServer);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings.NTPServerNotFound, oldNTPServer);

            return;
        }

        ntpServers[index] = newNTPServer;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.NTPServerModified, oldNTPServer, newNTPServer);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.NTPServers.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings.NTPServersNotConfigured);
        }
        else
        {
            new ListOutput()
                .WithTitle(CommandLineStrings.NTPServers)
                .WithEach(config.NetworkNTPSettings.NTPServers.Select(CliOutputValue.Create))
                .WriteLine();
        }

        new ListOutput()
            .WithTitle(CommandLineStrings.NTPFallbackServers)
            .WithEach((config.NetworkNTPSettings.FallbackNTPServers ?? []).Select(CliOutputValue.Create))
            .WriteLine();
    }
}
