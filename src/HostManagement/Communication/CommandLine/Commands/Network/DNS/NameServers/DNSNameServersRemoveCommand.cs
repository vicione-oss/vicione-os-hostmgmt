using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers remove command.
/// </summary>
internal sealed class DNSNameServersRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSNameServersRemoveCommand"/>.
    /// </summary>
    /// <param name="nameServersOption">The name servers option.</param>
    public DNSNameServersRemoveCommand(Option<IPAddress[]> nameServersOption) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_DNS.DNSNameServersRemoveCommandDescription)
    {
        Add(nameServersOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(nameServersOption),
                cancellationToken);
        });
    }

    private static async Task RemoveHandler(IPAddress[] nameServers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
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

            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.NameServerRemoved
                : CommandLineStrings_DNS.NameServersRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}
