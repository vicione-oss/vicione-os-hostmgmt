using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Validators;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the DNS name servers command.
/// </summary>
internal sealed class DNSNameServersCommand : ConfigCommandBase
{
    private static readonly Option<IPAddress> s_oldNameServerOption = new Option<IPAddress>(["-o", "--old-name-server"])
    {
        Description = "The old name server.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "Old name server"));

    private static readonly Option<IPAddress> s_newNameServerOption = new Option<IPAddress>(["-n", "--new-name-server"])
    {
        Description = "The new name server.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "New name server"));

    private static readonly Option<IPAddress[]> s_nameServerItemsOption = CommonOptions.s_ipAddressItemsOption;

    /// <summary>
    /// Initializes a new <see cref="DNSNameServersCommand"/>.
    /// </summary>
    public DNSNameServersCommand() : base(
        CommandLineStrings_DNS.DNSNameServersCommandName,
        CommandLineStrings_DNS.DNSNameServersCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_DNS.DNSNameServersEnableCommandDescription,
            () => EnableDisableHandler(enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_DNS.DNSNameServersDisableCommandDescription,
            () => EnableDisableHandler(enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_DNS.DNSNameServersListCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_Common.AddCommandName,
            CommandLineStrings_DNS.DNSNameServersAddCommandDescription,
            s_nameServerItemsOption,
            AddHandler);

        Add(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_DNS.DNSNameServersRemoveCommandDescription,
            s_nameServerItemsOption,
            RemoveHandler);

        Add(
            CommandLineStrings_Common.SetCommandName,
            CommandLineStrings_DNS.DNSNameServersSetCommandDescription,
            s_nameServerItemsOption,
            SetHandler);

        Add(
            CommandLineStrings_Common.ClearCommandName,
            CommandLineStrings_DNS.DNSNameServersClearCommandDescription,
            ClearHandler);

        Add(
            CommandLineStrings_Common.ModifyCommandName,
            CommandLineStrings_DNS.DNSNameServersModifyCommandDescription,
            s_oldNameServerOption,
            s_newNameServerOption,
            ModifyHandler);
    }

    private static async Task EnableDisableHandler(bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServersEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_DNS.NameServersAlreadyEnabled
                : CommandLineStrings_DNS.NameServersAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        if (enable &&
            config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.NameServersNotConfigured);
            return;
        }

        config.NetworkDNSSettings.NameServersEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_DNS.NameServersEnabled
            : CommandLineStrings_DNS.NameServersDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task AddHandler(IPAddress[] nameServers)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedNameServer = false;
        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            if (config.NetworkDNSSettings.NameServers.Contains(nameServer))
            {
                output.WithInformation(CommandLineStrings_DNS.NameServerAlreadyDefined, nameServer);
                continue;
            }

            config.NetworkDNSSettings.NameServers.Add(nameServer);
            output.With(nameServer);

            hasAddedNameServer = true;
        }

        if (hasAddedNameServer)
        {
            if (!await SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.NameServerAdded
                : CommandLineStrings_DNS.NameServersAdded;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.NameServersEnabled)
                output.WithWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(IPAddress[] nameServers)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedNameServer = false;
        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            if (!config.NetworkDNSSettings.NameServers.Contains(nameServer))
            {
                output.WithWarning(CommandLineStrings_DNS.NameServerNotFound, nameServer);
                continue;
            }

            if (!config.NetworkDNSSettings.NameServers.Remove(nameServer))
            {
                output
                    .WithError(CommandLineStrings_DNS.NameServerCouldNotBeRemoved, nameServer)
                    .WriteLine();

                return;
            }

            output.With(nameServer);

            hasRemovedNameServer = true;
        }

        if (hasRemovedNameServer)
        {
            if (config.NetworkDNSSettings.NameServersEnabled)
            {
                if (config.NetworkDNSSettings.NameServers.Count == 0)
                {
                    config.NetworkDNSSettings.NameServersEnabled = false;

                    output.WithInformation(CommandLineStrings_DNS.NameServersDisabled);
                }
            }
            else
            {
                if (config.NetworkDNSSettings.NameServers.Count > 0)
                {
                    output.WithWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.NameServerRemoved
                : CommandLineStrings_DNS.NameServersRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }

    private static async Task SetHandler(IPAddress[] nameServers)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.NameServers.Clear();

        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            config.NetworkDNSSettings.NameServers.Add(nameServer);
            output.With(nameServer);
        }

        if (!await SetConfiguration(config))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_DNS.NameServerSet
            : CommandLineStrings_DNS.NameServersSet;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.NameServersEnabled)
            output.WithWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersNotConfigured);
            return;
        }

        config.NetworkDNSSettings.NameServers.Clear();
        config.NetworkDNSSettings.NameServersEnabled = false;

        if (!await SetConfiguration(config))
            return;

        SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersDisabled);
        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServersCleared);
    }

    private static async Task ModifyHandler(IPAddress oldNameServer, IPAddress newNameServer)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var nameServers = config.NetworkDNSSettings.NameServers;

        if (nameServers.Contains(newNameServer))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServerAlreadyDefined, newNameServer);

            return;
        }

        var index = nameServers.IndexOf(oldNameServer);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.NameServerNotFound, oldNameServer);

            return;
        }

        nameServers[index] = newNameServer;

        if (!await SetConfiguration(config))
            return;

        if (!config.NetworkDNSSettings.NameServersEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServerModified, oldNameServer, newNameServer);
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersNotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings_DNS.NameServers, config.NetworkDNSSettings.NameServersEnabled)
            .WithEach(config.NetworkDNSSettings.NameServers.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
