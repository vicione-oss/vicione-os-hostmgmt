using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers list command.
/// </summary>
internal sealed class NameServersListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NameServersListCommand"/>.
    /// </summary>
    public NameServersListCommand()
        : base(CliStrings.Common.ListCommandName, CliStrings.Network.DNS.NameServers.ListCommandDescription)
        => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Addresses.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.NameServers.NotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CliStrings.Network.DNS.NameServers.Label, config.NetworkDNSSettings.NameServers.Enabled)
            .WithEach(config.NetworkDNSSettings.NameServers.Addresses.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
