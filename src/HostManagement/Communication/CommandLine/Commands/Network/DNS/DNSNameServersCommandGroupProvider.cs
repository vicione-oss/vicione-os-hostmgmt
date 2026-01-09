using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Validators;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS;

/// <summary>
/// Represents the <see cref="DNSNameServersCommandGroupProvider"/>.
/// </summary>
internal static class DNSNameServersCommandGroupProvider
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
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServersEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.DNSNameServersAlreadyEnabled
                : CommandLineStrings.DNSNameServersAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info);

            return;
        }

        config.NetworkDNSSettings.NameServersEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (enable &&
            config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.DNSNameServersNotConfigured);
        }

        var enableMessage = enable
            ? CommandLineStrings.DNSNameServersEnabled
            : CommandLineStrings.DNSNameServersDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }

    private static async Task AddHandler(IPAddress[] nameServers)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasAddedNameServer = false;
        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            if (config.NetworkDNSSettings.NameServers.Contains(nameServer))
            {
                output.WithInformation(CommandLineStrings.DNSNameServerAlreadyDefined, nameServer);
                continue;
            }

            config.NetworkDNSSettings.NameServers.Add(nameServer);
            output.With(nameServer);

            hasAddedNameServer = true;
        }

        if (hasAddedNameServer)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.DNSNameServerAdded
                : CommandLineStrings.DNSNameServersAdded;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.NameServersEnabled)
                output.WithWarning(CommandLineStrings.DNSNameServersCurrentlyDisabled);
        }

        output.WriteLine();
    }

    private static async Task RemoveHandler(IPAddress[] nameServers)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var hasRemovedNameServer = false;
        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            if (!config.NetworkDNSSettings.NameServers.Contains(nameServer))
            {
                output.WithWarning(CommandLineStrings.DNSNameServerNotFound, nameServer);
                continue;
            }

            if (!config.NetworkDNSSettings.NameServers.Remove(nameServer))
            {
                output
                    .WithError(CommandLineStrings.DNSNameServerCouldNotBeRemoved, nameServer)
                    .WriteLine();

                return;
            }

            output.With(nameServer);

            hasRemovedNameServer = true;
        }

        if (hasRemovedNameServer)
        {
            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings.DNSNameServerRemoved
                : CommandLineStrings.DNSNameServersRemoved;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.NameServersEnabled &&
                config.NetworkDNSSettings.NameServers.Count > 0)
            {
                output.WithWarning(CommandLineStrings.DNSNameServersCurrentlyDisabled);
            }
        }

        output.WriteLine();
    }

    private static async Task SetHandler(IPAddress[] nameServers)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.NameServers.Clear();

        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            config.NetworkDNSSettings.NameServers.Add(nameServer);
            output.With(nameServer);
        }

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings.DNSNameServerSet
            : CommandLineStrings.DNSNameServersSet;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.NameServersEnabled)
            output.WithWarning(CommandLineStrings.DNSNameServersCurrentlyDisabled);

        output.WriteLine();
    }

    private static async Task ClearHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.DNSNameServersNotConfigured);

            return;
        }

        config.NetworkDNSSettings.NameServers.Clear();

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.DNSNameServersCleared);
    }

    private static async Task ModifyHandler(IPAddress oldNameServer, IPAddress newNameServer)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var nameServers = config.NetworkDNSSettings.NameServers;

        if (nameServers.Contains(newNameServer))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.DNSNameServerAlreadyDefined, newNameServer);

            return;
        }

        var index = nameServers.IndexOf(oldNameServer);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings.DNSNameServerNotFound, oldNameServer);

            return;
        }

        nameServers[index] = newNameServer;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (!config.NetworkDNSSettings.NameServersEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings.DNSNameServersCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings.DNSNameServerModified, oldNameServer, newNameServer);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings.DNSNameServersNotConfigured);

            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings.DNSNameServers, config.NetworkDNSSettings.NameServersEnabled)
            .WithEach(config.NetworkDNSSettings.NameServers.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
