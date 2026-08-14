using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers enable command.
/// </summary>
internal sealed class NameServersEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NameServersEnableCommand"/>.
    /// </summary>
    public NameServersEnableCommand()
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.DNS.NameServers.EnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.NameServers.AlreadyEnabled);
            return;
        }

        if (config.NetworkDNSSettings.NameServers.Addresses.Count == 0)
        {
            SimpleOutput.WriteLineError(CliStrings.Network.DNS.NameServers.NotConfigured);
            return;
        }

        config.NetworkDNSSettings.NameServers.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.NameServers.Enabled);
    }
}
