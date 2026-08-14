using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts clear command.
/// </summary>
internal sealed class StaticHostsClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="StaticHostsClearCommand"/>.
    /// </summary>
    public StaticHostsClearCommand()
        : base(CliStrings.Common.ClearCommandName, CliStrings.Network.DNS.StaticHosts.ClearCommandDescription)
        => SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Hosts.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.StaticHosts.NotConfigured);
            return;
        }

        config.NetworkDNSSettings.StaticHosts.Hosts.Clear();
        config.NetworkDNSSettings.StaticHosts.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.StaticHosts.Disabled);
        SimpleOutput.WriteLine(CliStrings.Network.DNS.StaticHosts.Cleared);
    }
}
