using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
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
    public StaticHostsRemoveCommand(Option<IPAddress> addressOption, Option<string> hostnameOption) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_DNS.DNSStaticHostsRemoveCommandDescription)
    {
        Add(addressOption);
        Add(hostnameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(addressOption),
                parseResult.GetRequiredValue(hostnameOption),
                cancellationToken);
        });
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

        var output = new TableOutput();

        if (!config.NetworkDNSSettings.StaticHosts.Hosts.Contains(staticHostToRemove))
        {
            output
                .WithWarning(CommandLineStrings_DNS.StaticHostNotFound, address, hostname)
                .WriteLine();

            return;
        }

        if (!config.NetworkDNSSettings.StaticHosts.Hosts.Remove(staticHostToRemove))
        {
            output
                .WithError(CommandLineStrings_DNS.StaticHostCouldNotBeRemoved, address, hostname)
                .WriteLine();

            return;
        }

        output.With(address, hostname);

        if (config.NetworkDNSSettings.StaticHosts.Enabled)
        {
            if (config.NetworkDNSSettings.StaticHosts.Hosts.Count == 0)
            {
                config.NetworkDNSSettings.StaticHosts.Enabled = false;

                output.WithInformation(CommandLineStrings_DNS.StaticHostsDisabled);
            }
        }
        else
        {
            if (config.NetworkDNSSettings.StaticHosts.Hosts.Count > 0)
                output.WithWarning(CommandLineStrings_DNS.StaticHostsCurrentlyDisabled);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        output.WithTitle(CommandLineStrings_DNS.StaticHostRemoved).WriteLine();
    }
}
