using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts list command.
/// </summary>
internal sealed class DNSStaticHostsListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSStaticHostsListCommand"/>.
    /// </summary>
    public DNSStaticHostsListCommand() : base(
        CommandLineStrings_Common.ListCommandName,
        CommandLineStrings_DNS.DNSStaticHostsListCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));
    }

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
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
