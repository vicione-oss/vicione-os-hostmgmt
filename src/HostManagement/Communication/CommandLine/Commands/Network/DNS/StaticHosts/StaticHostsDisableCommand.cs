using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts disable command.
/// </summary>
internal sealed class StaticHostsDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="StaticHostsDisableCommand"/>.
    /// </summary>
    public StaticHostsDisableCommand()
        : base(
            CliStrings.Common.DisableCommandName,
            CommandLineStrings_DNS.DNSStaticHostsDisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.StaticHosts.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsAlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.StaticHosts.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.StaticHostsDisabled);
    }
}
