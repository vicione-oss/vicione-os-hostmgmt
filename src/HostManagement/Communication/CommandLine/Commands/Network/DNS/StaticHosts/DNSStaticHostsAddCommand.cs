using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts add command.
/// </summary>
internal sealed class DNSStaticHostsAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSStaticHostsAddCommand"/>.
    /// </summary>
    /// <param name="addressOption">The IP address option.</param>
    /// <param name="hostnameOption">The hostname option.</param>
    public DNSStaticHostsAddCommand(Option<IPAddress> addressOption, Option<string> hostnameOption) : base(
        CommandLineStrings_Common.AddCommandName,
        CommandLineStrings_DNS.DNSStaticHostsAddCommandDescription)
    {
        Add(addressOption);
        Add(hostnameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return AddHandler(
                parseResult.GetRequiredValue(addressOption),
                parseResult.GetRequiredValue(hostnameOption),
                cancellationToken);
        });
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

        if (!await SetConfiguration(config, cancellationToken))
            return;

        output.WithTitle(CommandLineStrings_DNS.StaticHostAdded);

        if (!config.NetworkDNSSettings.StaticHostsEnabled)
            output.WithWarning(CommandLineStrings_DNS.StaticHostsCurrentlyDisabled);

        output.WriteLine();
    }
}
