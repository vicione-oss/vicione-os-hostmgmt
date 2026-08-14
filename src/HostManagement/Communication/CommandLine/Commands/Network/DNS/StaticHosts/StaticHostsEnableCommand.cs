using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public StaticHostsEnableCommand()
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.DNS.StaticHosts.EnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.StaticHosts.AlreadyEnabled);
            return;
        }

        if (config.NetworkDNSSettings.StaticHosts.Hosts.Count == 0)
        {
            SimpleOutput.WriteLineError(CliStrings.Network.DNS.StaticHosts.NotConfigured);
            return;
        }

        config.NetworkDNSSettings.StaticHosts.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.StaticHosts.Enabled);
    }
}
