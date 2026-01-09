using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the static hosts command.
/// </summary>
internal sealed class DNSStaticHostsCommand : ConfigCommandBase
{
    private static readonly Option<IPAddress> s_addressOption = CommonOptions.s_ipAddressOption;
    private static readonly Option<string> s_hostnameOption = new(["-n", "--hostname"])
    {
        Description = "The hostname.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="DNSStaticHostsCommand"/>.
    /// </summary>
    public DNSStaticHostsCommand() : base(
        CommandLineStrings_DNS.DNSStaticHostsCommandName,
        CommandLineStrings_DNS.DNSStaticHostsCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_DNS.DNSStaticHostsEnableCommandDescription,
            () => EnableDisableHandler(enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_DNS.DNSStaticHostsDisableCommandDescription,
            () => EnableDisableHandler(enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_DNS.DNSStaticHostsListCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_Common.AddCommandName,
            CommandLineStrings_DNS.DNSStaticHostsAddCommandDescription,
            s_addressOption,
            s_hostnameOption,
            AddHandler);

        Add(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_DNS.DNSStaticHostsRemoveCommandDescription,
            s_addressOption,
            s_hostnameOption,
            RemoveHandler);

        Add(
            CommandLineStrings_Common.ClearCommandName,
            CommandLineStrings_DNS.DNSStaticHostsClearCommandDescription,
            ClearHandler);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHostsEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_DNS.StaticHostsAlreadyEnabled
                : CommandLineStrings_DNS.StaticHostsAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);
            return;
        }

        if (enable &&
            config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.StaticHostsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.StaticHostsEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_DNS.StaticHostsEnabled
            : CommandLineStrings_DNS.StaticHostsDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task AddHandler(IPAddress address, string hostname)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
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
                .WithInformation(CommandLineStrings_DNS.StaticHostAlreadyDefined, address, hostname)
                .WriteLine();

            return;
        }

        config.NetworkDNSSettings.StaticHosts.Add(newStaticHost);
        output.With(address, hostname);

        if (!await SetConfiguration(config))
            return;

        output.WithTitle(CommandLineStrings_DNS.StaticHostAdded);

        if (!config.NetworkDNSSettings.StaticHostsEnabled)
            output.WithWarning(CommandLineStrings_DNS.StaticHostsCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task RemoveHandler(IPAddress address, string hostname)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
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
                .WithWarning(CommandLineStrings_DNS.StaticHostNotFound, address, hostname)
                .WriteLine();

            return;
        }

        if (!config.NetworkDNSSettings.StaticHosts.Remove(staticHostToRemove))
        {
            output
                .WithError(CommandLineStrings_DNS.StaticHostCouldNotBeRemoved, address, hostname)
                .WriteLine();

            return;
        }

        output.With(address, hostname);

        if (config.NetworkDNSSettings.StaticHostsEnabled)
        {
            if (config.NetworkDNSSettings.StaticHosts.Count == 0)
            {
                config.NetworkDNSSettings.StaticHostsEnabled = false;

                output.WithInformation(CommandLineStrings_DNS.StaticHostsDisabled);
            }
        }
        else
        {
            if (config.NetworkDNSSettings.StaticHosts.Count > 0)
                output.WithWarning(CommandLineStrings_DNS.StaticHostsCurrentlyDisabled);
        }

        if (!await SetConfiguration(config))
            return;

        output.WithTitle(CommandLineStrings_DNS.StaticHostRemoved).WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.StaticHosts.Clear();
        config.NetworkDNSSettings.StaticHostsEnabled = false;

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsDisabled);
        SimpleOutput.WriteLine(CommandLineStrings_DNS.StaticHostsCleared);
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsNotConfigured);
            return;
        }

        var output = new TableOutput()
            .WithTitle(
                CommandLineStrings_DNS.StaticHosts,
                config.NetworkDNSSettings.StaticHostsEnabled);

        config.NetworkDNSSettings.StaticHosts.ForEach(host => output.With(host.IPAddress, host.Hostname));
        output.WriteLine();
    }
}
