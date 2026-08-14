using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts add command.
/// </summary>
internal sealed class StaticHostsAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="StaticHostsAddCommand"/>.
    /// </summary>
    /// <param name="addressOption">The IP address option.</param>
    /// <param name="hostnameOption">The hostname option.</param>
    public StaticHostsAddCommand(Option<IPAddress> addressOption, Option<string> hostnameOption)
        : base(CliStrings.Common.AddCommandName, CliStrings.Network.DNS.StaticHosts.AddCommandDescription)
    {
        Add(addressOption);
        Add(hostnameOption);

        SetAction((parseResult, cancellationToken)
            => AddHandler(
                parseResult.GetRequiredValue(addressOption),
                parseResult.GetRequiredValue(hostnameOption),
                cancellationToken));
    }

    private static async Task AddHandler(IPAddress address, string hostname, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var newStaticHost = new StaticHostDetail()
        {
            IPAddress = address,
            Hostname = hostname
        };

        if (config.NetworkDNSSettings.StaticHosts.Hosts.Contains(newStaticHost))
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.StaticHosts.AlreadyDefined, address, hostname);
            return;
        }

        config.NetworkDNSSettings.StaticHosts.Hosts.Add(newStaticHost);

        var output = new TableOutput();
        output.With(address, hostname);

        if (!await SetConfiguration(config, cancellationToken))
            return;

        output.WithTitle(CliStrings.Network.DNS.StaticHosts.Added);

        if (!config.NetworkDNSSettings.StaticHosts.Enabled)
            output.WithWarning(CliStrings.Network.DNS.StaticHosts.CurrentlyDisabled);

        output.WriteLine();
    }
}
