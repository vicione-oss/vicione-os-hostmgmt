using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts list command.
/// </summary>
internal sealed class StaticHostsListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="StaticHostsListCommand"/>.
    /// </summary>
    public StaticHostsListCommand()
        : base(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_DNS.DNSStaticHostsListCommandDescription)
        => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Hosts.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsNotConfigured);
            return;
        }

        var output = new TableOutput()
            .WithTitle(CommandLineStrings_DNS.StaticHosts, config.NetworkDNSSettings.StaticHosts.Enabled);

        config.NetworkDNSSettings.StaticHosts.Hosts.ForEach(host => output.With(host.IPAddress, host.Hostname));
        output.WriteLine();
    }
}
