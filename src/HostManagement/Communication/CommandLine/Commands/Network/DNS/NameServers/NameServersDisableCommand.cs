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
        : base(
            CliStrings.Common.DisableCommandName,
            CommandLineStrings_DNS.DNSNameServersDisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.NameServers.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersAlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.NameServers.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServersDisabled);
    }
}
