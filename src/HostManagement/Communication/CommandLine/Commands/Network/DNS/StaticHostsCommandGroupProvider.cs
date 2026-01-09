using System.CommandLine;
using System.Net;
using System.Text;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="StaticHostsCommandGroupProvider"/>.
/// </summary>
internal static class StaticHostsCommandGroupProvider
{
    private static readonly Option<IPAddress> s_addressOption = CommonOptions.s_ipAddressOption;
    private static readonly Option<string> s_hostnameOption = new(["-n", "--hostname"])
    {
        Description = "The hostname.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable static hosts.",
        () => EnableDisableHandler(enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable static hosts.",
        () => EnableDisableHandler(enable: false));

    private static readonly Command s_addCommand = CommandHelper.ConstructCommand(
        "add",
        "Add a new static host entry.",
        s_addressOption,
        s_hostnameOption,
        AddHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove a static host entry.",
        s_addressOption,
        s_hostnameOption,
        RemoveHandler);

    private static readonly Command s_clearCommand = CommandHelper.ConstructCommand(
        "clear",
        "Remove all static host entries.",
        ClearHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List all configured static hosts.",
        ListHandler);

    /// <summary>
    /// Adds the command group to configure static hosts.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddStaticHostsCommandGroup(this Command parentCommand)
    {
        var staticHostsCommand = parentCommand.AddCommand(
            "static-hosts",
            "Configure static hosts. Specified hostnames are mapped to IP addresses.");

        staticHostsCommand.AddCommand(s_enableCommand);
        staticHostsCommand.AddCommand(s_disableCommand);

        staticHostsCommand.AddCommand(s_addCommand);
        staticHostsCommand.AddCommand(s_removeCommand);
        staticHostsCommand.AddCommand(s_clearCommand);
        staticHostsCommand.AddCommand(s_listCommand);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.StaticHostsEnabled = enable;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
            ? CommandLineStrings.DNSStaticHostsEnabled
            : CommandLineStrings.DNSStaticHostsDisabled);

        if (config.NetworkDNSSettings.StaticHosts.Count == 0)
            ConsoleEx.WriteLineWarning(CommandLineStrings.DNSStaticHostsNotDefined);
    }

    private static async Task AddHandler(
        IPAddress addressOptionValue,
        string hostnameOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var newStaticHost = new StaticHostDetail()
        {
            IPAddress = addressOptionValue,
            Hostname = hostnameOptionValue
        };

        if (config.NetworkDNSSettings.StaticHosts.Contains(newStaticHost))
        {
            ConsoleEx.WriteLineWarning(
                CommandLineStrings.DNSStaticHostAlreadyDefined,
                addressOptionValue,
                hostnameOptionValue);

            return;
        }

        config.NetworkDNSSettings.StaticHosts.Add(newStaticHost);

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.DNSStaticHostAdded,
            addressOptionValue,
            hostnameOptionValue);

        if (!config.NetworkDNSSettings.StaticHostsEnabled)
            ConsoleEx.WriteLineWarning(CommandLineStrings.DNSStaticHostsCurrentlyDisabled);
    }

    private static async Task RemoveHandler(
        IPAddress addressOptionValue,
        string hostnameOptionValue)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var staticHostToRemove = new StaticHostDetail()
        {
            IPAddress = addressOptionValue,
            Hostname = hostnameOptionValue
        };

        if (!config.NetworkDNSSettings.StaticHosts.Contains(staticHostToRemove))
        {
            ConsoleEx.WriteLineWarning(
                CommandLineStrings.DNSStaticHostNotFound,
                addressOptionValue,
                hostnameOptionValue);

            return;
        }

        if (!config.NetworkDNSSettings.StaticHosts.Remove(staticHostToRemove))
        {
            ConsoleEx.WriteLineError(
                CommandLineStrings.DNSStaticHostCouldNotBeRemoved,
                staticHostToRemove.IPAddress,
                staticHostToRemove.Hostname);

            return;
        }

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.DNSStaticHostRemoved,
            staticHostToRemove.IPAddress,
            staticHostToRemove.Hostname);

        if (!config.NetworkDNSSettings.StaticHostsEnabled)
            ConsoleEx.WriteLineWarning(CommandLineStrings.DNSStaticHostsCurrentlyDisabled);
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.StaticHosts.Clear();

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(CommandLineStrings.DNSStaticHostsCleared);

        if (!config.NetworkDNSSettings.StaticHostsEnabled)
            ConsoleEx.WriteLineWarning(CommandLineStrings.DNSStaticHostsCurrentlyDisabled);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            Console.WriteLine(CommandLineStrings.DNSStaticHostsNotDefined);
            return;
        }

        var builder = new StringBuilder();

        builder.AppendLine($"Static hosts ({(config.NetworkDNSSettings.StaticHostsEnabled ? "enabled" : "disabled")})");

        foreach (var staticHost in config.NetworkDNSSettings.StaticHosts)
            builder.AppendLine($"{staticHost.IPAddress}  {staticHost.Hostname}");

        Console.WriteLine(builder.ToString().TrimEnd('\r', '\n'));
    }
}
