using System.Net;
using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers modify command.
/// </summary>
internal sealed class DNSNameServersModifyCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSNameServersModifyCommand"/>.
    /// </summary>
    /// <param name="oldNameServerOption">The old name server option.</param>
    /// <param name="newNameServerOption">The new name server option.</param>
    public DNSNameServersModifyCommand(
        Option<IPAddress> oldNameServerOption,
        Option<IPAddress> newNameServerOption)
        : base(
            CommandLineStrings_Common.ModifyCommandName,
            CommandLineStrings_DNS.DNSNameServersModifyCommandDescription)
    {
        Add(oldNameServerOption);
        Add(newNameServerOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return ModifyHandler(
                parseResult.GetRequiredValue(oldNameServerOption),
                parseResult.GetRequiredValue(newNameServerOption),
                cancellationToken);
        });
    }

    private static async Task ModifyHandler(
        IPAddress oldNameServer,
        IPAddress newNameServer,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
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

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (!config.NetworkDNSSettings.NameServersEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServerModified, oldNameServer, newNameServer);
    }
}
