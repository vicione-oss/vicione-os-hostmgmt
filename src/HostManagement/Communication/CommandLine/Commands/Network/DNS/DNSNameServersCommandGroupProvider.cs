using System.CommandLine;
using System.Net;
using System.Text;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="DNSNameServersCommandGroupProvider"/>.
/// </summary>
internal static class DNSNameServersCommandGroupProvider
{
    private static readonly Option<IPAddress> s_oldNameServerOption = new(["-o", "--old-name-server"])
    {
        Description = "The old name server.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<IPAddress> s_newNameServerOption = new(["-n", "--new-name-server"])
    {
        Description = "The new name server.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<IPAddress[]> s_nameServerItemsOption = CommonOptions.s_ipAddressItemsOption;

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable DNS name servers.",
        () => EnableDisableHandler(enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable DNS name servers.",
        () => EnableDisableHandler(enable: false));

    private static readonly Command s_addCommand = CommandHelper.ConstructCommand(
        "add",
        "Add new DNS name servers.",
        s_nameServerItemsOption,
        AddHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove existing DNS name servers.",
        s_nameServerItemsOption,
        RemoveHandler);

    private static readonly Command s_setCommand = CommandHelper.ConstructCommand(
        "set",
        "Remove existing DNS name servers and set new ones.",
        s_nameServerItemsOption,
        SetHandler);

    private static readonly Command s_clearCommand = CommandHelper.ConstructCommand(
        "clear",
        "Remove all DNS name servers.",
        ClearHandler);

    private static readonly Command s_modifyCommand = CommandHelper.ConstructCommand(
        "modify",
        "Modify an existing DNS name server.",
        s_oldNameServerOption,
        s_newNameServerOption,
        ModifyHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List all DNS name servers.",
        ListHandler);

    /// <summary>
    /// Adds the command group to configure DNS name servers.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddDNSNameServersCommandGroup(this Command parentCommand)
    {
        var nameServersCommand = parentCommand.AddCommand(
            "name-servers",
            "Configure DNS name servers.");

        nameServersCommand.AddCommand(s_enableCommand);
        nameServersCommand.AddCommand(s_disableCommand);

        nameServersCommand.AddCommand(s_addCommand);
        nameServersCommand.AddCommand(s_removeCommand);
        nameServersCommand.AddCommand(s_setCommand);
        nameServersCommand.AddCommand(s_clearCommand);
        nameServersCommand.AddCommand(s_modifyCommand);
        nameServersCommand.AddCommand(s_listCommand);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.NameServersEnabled = enable;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
            ? CommandLineStrings.DNSNameServersEnabled
            : CommandLineStrings.DNSNameServersDisabled);
    }

    private static async Task AddHandler(IPAddress[] nameServerItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        for (var i = 0; i < nameServerItemsOptionValue.Length; i++)
        {
            if (config.NetworkDNSSettings.NameServers.Contains(nameServerItemsOptionValue[i]))
            {
                ConsoleEx.WriteLineWarning(
                    CommandLineStrings.DNSNameServerAlreadyDefined,
                    nameServerItemsOptionValue[i].ToString());

                continue;
            }

            config.NetworkDNSSettings.NameServers.Add(nameServerItemsOptionValue[i]);
            builder.Append(nameServerItemsOptionValue[i]);

            if (i < nameServerItemsOptionValue.Length - 1)
            {
                builder.Append(", ");
            }
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        if (builder.Length > 0)
            Console.WriteLine(CommandLineStrings.DNSNameServersAdded, builder.ToString());
    }

    private static async Task RemoveHandler(IPAddress[] nameServerItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new StringBuilder();

        for (var i = 0; i < nameServerItemsOptionValue.Length; i++)
        {
            if (!config.NetworkDNSSettings.NameServers.Contains(nameServerItemsOptionValue[i]))
            {
                ConsoleEx.WriteLineWarning(
                    CommandLineStrings.DNSNameServerNotFound,
                    nameServerItemsOptionValue[i].ToString());

                continue;
            }

            if (!config.NetworkDNSSettings.NameServers.Remove(nameServerItemsOptionValue[i]))
            {
                ConsoleEx.WriteLineError(
                    CommandLineStrings.DNSNameServerCouldNotBeRemoved,
                    nameServerItemsOptionValue[i].ToString());

                return;
            }

            builder.Append(nameServerItemsOptionValue[i]);

            if (i < nameServerItemsOptionValue.Length - 1)
            {
                builder.Append(", ");
            }
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        if (builder.Length > 0)
            Console.WriteLine(CommandLineStrings.DNSNameServersRemoved, builder.ToString());
    }

    private static async Task SetHandler(IPAddress[] nameServerItemsOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.NameServers.Clear();

        var builder = new StringBuilder();

        for (var i = 0; i < nameServerItemsOptionValue.Length; i++)
        {
            config.NetworkDNSSettings.NameServers.Add(nameServerItemsOptionValue[i]);
            builder.Append(nameServerItemsOptionValue[i]);

            if (i < nameServerItemsOptionValue.Length - 1)
            {
                builder.Append(", ");
            }
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.DNSNameServersSet,
            builder.ToString());
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.NameServers.Clear();

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.DNSNameServersCleared);
    }

    private static async Task ModifyHandler(IPAddress oldNameServerOptionValue, IPAddress newNameServerOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var nameServers = config.NetworkDNSSettings.NameServers;

        if (nameServers.Contains(newNameServerOptionValue))
        {
            ConsoleEx.WriteLineWarning(CommandLineStrings.DNSNameServerAlreadyDefined, newNameServerOptionValue);
            return;
        }

        var index = nameServers.IndexOf(oldNameServerOptionValue);

        if (index == -1)
        {
            ConsoleEx.WriteLineError(CommandLineStrings.DNSNameServerNotFound, oldNameServerOptionValue);
            return;
        }

        nameServers[index] = newNameServerOptionValue;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.DNSNameServerModified, oldNameServerOptionValue, newNameServerOptionValue);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Count == 0)
        {
            Console.WriteLine(CommandLineStrings.DNSNameServersNotDefined);
            return;
        }

        foreach (var nameServer in config.NetworkDNSSettings.NameServers)
            Console.WriteLine(nameServer);
    }
}
