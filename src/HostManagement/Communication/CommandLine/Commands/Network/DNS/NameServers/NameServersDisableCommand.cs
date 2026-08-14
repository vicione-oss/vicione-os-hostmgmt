using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers disable command.
/// </summary>
internal sealed class NameServersDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NameServersDisableCommand"/>.
    /// </summary>
    public NameServersDisableCommand()
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.DNS.NameServers.DisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.NameServers.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.NameServers.AlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.NameServers.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.NameServers.Disabled);
    }
}
