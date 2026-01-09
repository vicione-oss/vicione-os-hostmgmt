using System.CommandLine;
using System.Text;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
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

    private static async Task AddHandler(string[] ntpServerItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        for (var i = 0; i < ntpServerItemsOptionValue.Length; i++)
        {
            config.NetworkNTPSettings.NTPServers.Add(ntpServerItemsOptionValue[i]);
            builder.Append(ntpServerItemsOptionValue[i]);

            if (i < ntpServerItemsOptionValue.Length - 1)
            {
                builder.Append(", ");
            }
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        if (builder.Length > 0)
            Console.WriteLine(CommandLineStrings.NTPServersAdded, builder.ToString());
    }

    private static async Task RemoveHandler(string[] ntpServerItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        for (var i = 0; i < ntpServerItemsOptionValue.Length; i++)
        {
            if (!config.NetworkNTPSettings.NTPServers.Contains(ntpServerItemsOptionValue[i]))
            {
                ConsoleEx.WriteLineWarning(
                    CommandLineStrings.NTPServerNotFound,
                    ntpServerItemsOptionValue[i].ToString());

                continue;
            }

            if (!config.NetworkNTPSettings.NTPServers.Remove(ntpServerItemsOptionValue[i]))
            {
                ConsoleEx.WriteLineError(
                    CommandLineStrings.NTPServerCouldNotBeRemoved,
                    ntpServerItemsOptionValue[i].ToString());

                return;
            }

            builder.Append(ntpServerItemsOptionValue[i] + ", ");
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        if (builder.Length > 0)
        {
            Console.WriteLine(
                CommandLineStrings.NTPServersRemoved,
                builder.ToString().TrimEnd().TrimEnd(','));
        }
    }

    private static async Task SetHandler(string[] ntpServerItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkNTPSettings.NTPServers.Clear();

        var builder = new StringBuilder();

        for (var i = 0; i < ntpServerItemsOptionValue.Length; i++)
        {
            config.NetworkNTPSettings.NTPServers.Add(ntpServerItemsOptionValue[i]);
            builder.Append(ntpServerItemsOptionValue[i]);

            if (i < ntpServerItemsOptionValue.Length - 1)
            {
                builder.Append(", ");
            }
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.NTPServersSet,
            builder.ToString());
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkNTPSettings.NTPServers.Count == 0)
        {
            Console.WriteLine(CommandLineStrings.NTPServersNotDefined);
            return;
        }

        config.NetworkNTPSettings.NTPServers.Clear();

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.NTPServersCleared);
    }

    private static async Task ModifyHandler(string oldNTPServerOptionValue, string newNTPServerOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var ntpServers = config.NetworkNTPSettings.NTPServers;

        var index = ntpServers.IndexOf(oldNTPServerOptionValue);

        if (index == -1)
        {
            ConsoleEx.WriteLineError(CommandLineStrings.NTPServerNotFound, oldNTPServerOptionValue);
            return;
        }

        ntpServers[index] = newNTPServerOptionValue;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.NTPServerModified, oldNTPServerOptionValue, newNTPServerOptionValue);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        builder.AppendLine("NTP servers");

        if (config.NetworkNTPSettings.NTPServers.Count == 0)
        {
            builder.AppendLine($"  {CommandLineStrings.NTPServersNotDefined}");
        }
        else
        {
            foreach (var ntpServer in config.NetworkNTPSettings.NTPServers)
                builder.AppendLine($"  {ntpServer}");
        }

        builder.AppendLine("Fallback NTP servers");

        foreach (var fallbackNTPServer in config.NetworkNTPSettings.FallbackNTPServers)
            builder.AppendLine($"  {fallbackNTPServer}");

        Console.WriteLine(builder.ToString());
    }
}
