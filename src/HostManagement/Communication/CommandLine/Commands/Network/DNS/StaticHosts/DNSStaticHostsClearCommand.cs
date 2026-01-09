using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts clear command.
/// </summary>
internal sealed class DNSStaticHostsClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSStaticHostsClearCommand"/>.
    /// </summary>
    public DNSStaticHostsClearCommand() : base(
        CommandLineStrings_Common.ClearCommandName,
        CommandLineStrings_DNS.DNSStaticHostsClearCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));
    }

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.StaticHosts.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.StaticHosts.Clear();
        config.NetworkDNSSettings.StaticHostsEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.StaticHostsDisabled);
        SimpleOutput.WriteLine(CommandLineStrings_DNS.StaticHostsCleared);
    }
}
