using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts remove command.
/// </summary>
internal sealed class StaticHostsRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="StaticHostsRemoveCommand"/>.
    /// </summary>
    /// <param name="addressOption">The IP address option.</param>
    /// <param name="hostnameOption">The hostname option.</param>
    public StaticHostsRemoveCommand(Option<IPAddress> addressOption, Option<string> hostnameOption)
        : base(CliStrings.Common.RemoveCommandName, CliStrings.Network.DNS.StaticHosts.RemoveCommandDescription)
    {
        Add(addressOption);
        Add(hostnameOption);

        SetAction((parseResult, cancellationToken)
            => RemoveHandler(
                parseResult.GetRequiredValue(addressOption),
                parseResult.GetRequiredValue(hostnameOption),
                cancellationToken));
    }

    private static async Task RemoveHandler(IPAddress address, string hostname, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var staticHostToRemove = new StaticHostDetail()
        {
            IPAddress = address,
            Hostname = hostname
        };

        if (!config.NetworkDNSSettings.StaticHosts.Hosts.Contains(staticHostToRemove))
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.DNS.StaticHosts.NotFound, address, hostname);
            return;
        }

        if (!config.NetworkDNSSettings.StaticHosts.Hosts.Remove(staticHostToRemove))
        {
            SimpleOutput.WriteLineError(CliStrings.Network.DNS.StaticHosts.CouldNotBeRemoved, address, hostname);
            return;
        }

        var output = new TableOutput().With(address, hostname);

        if (config.NetworkDNSSettings.StaticHosts.Enabled)
        {
            if (config.NetworkDNSSettings.StaticHosts.Hosts.Count == 0)
            {
                config.NetworkDNSSettings.StaticHosts.Enabled = false;

                output.WithInformation(CliStrings.Network.DNS.StaticHosts.Disabled);
            }
        }
        else
        {
            if (config.NetworkDNSSettings.StaticHosts.Hosts.Count > 0)
                output.WithWarning(CliStrings.Network.DNS.StaticHosts.CurrentlyDisabled);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        output.WithTitle(CliStrings.Network.DNS.StaticHosts.Removed).WriteLine();
    }
}
