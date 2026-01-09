using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
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
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHostsEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.DNSStaticHostsAlreadyEnabled
                : CommandLineStrings.DNSStaticHostsAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        config.NetworkDNSSettings.StaticHostsEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (enable &&
            config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.DNSStaticHostsNotConfigured);
        }

        var enableMessage = enable
            ? CommandLineStrings.DNSStaticHostsEnabled
            : CommandLineStrings.DNSStaticHostsDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task AddHandler(IPAddress address, string hostname)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var newStaticHost = new StaticHostDetail()
        {
            IPAddress = address,
            Hostname = hostname
        };

        var output = new TableOutput();

        if (config.NetworkDNSSettings.StaticHosts.Contains(newStaticHost))
        {
            output
                .WithInformation(
                    CommandLineStrings.DNSStaticHostAlreadyDefined,
                    address,
                    hostname)
                .WriteLine();

            return;
        }

        config.NetworkDNSSettings.StaticHosts.Add(newStaticHost);
        output.With(address, hostname);

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        output.WithTitle(CommandLineStrings.DNSStaticHostAdded);

        if (!config.NetworkDNSSettings.StaticHostsEnabled)
            output.WithWarning(CommandLineStrings.DNSStaticHostsCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task RemoveHandler(IPAddress address, string hostname)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var staticHostToRemove = new StaticHostDetail()
        {
            IPAddress = address,
            Hostname = hostname
        };

        var output = new TableOutput();

        if (!config.NetworkDNSSettings.StaticHosts.Contains(staticHostToRemove))
        {
            output
                .WithWarning(
                    CommandLineStrings.DNSStaticHostNotFound,
                    address,
                    hostname)
                .WriteLine();

            return;
        }

        if (!config.NetworkDNSSettings.StaticHosts.Remove(staticHostToRemove))
        {
            output
                .WithError(
                    CommandLineStrings.DNSStaticHostCouldNotBeRemoved,
                    address,
                    hostname)
                .WriteLine();

            return;
        }

        output.With(address, hostname);

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        output.WithTitle(CommandLineStrings.DNSStaticHostRemoved);

        if (!config.NetworkDNSSettings.StaticHostsEnabled &&
            config.NetworkDNSSettings.StaticHosts.Count > 0)
        {
            output.WithWarning(CommandLineStrings.DNSStaticHostsCurrentlyDisabled);
        }

        output.WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.DNSStaticHostsNotConfigured);

            return;
        }

        config.NetworkDNSSettings.StaticHosts.Clear();

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.DNSStaticHostsCleared);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings.DNSStaticHostsNotConfigured);

            return;
        }

        var output = new TableOutput()
            .WithTitle(
                CommandLineStrings.DNSStaticHosts,
                config.NetworkDNSSettings.StaticHostsEnabled);

        config.NetworkDNSSettings.StaticHosts.ForEach(host => output.With(host.IPAddress, host.Hostname));
        output.WriteLine();
    }
}
