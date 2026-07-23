using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts enable command.
/// </summary>
internal sealed class StaticHostsEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="StaticHostsEnableCommand"/>.
    /// </summary>
    public StaticHostsEnableCommand() : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_DNS.DNSStaticHostsEnableCommandDescription)
            => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsAlreadyEnabled);
            return;
        }

        if (config.NetworkDNSSettings.StaticHosts.Hosts.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.StaticHostsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.StaticHosts.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.StaticHostsEnabled);
    }
}
