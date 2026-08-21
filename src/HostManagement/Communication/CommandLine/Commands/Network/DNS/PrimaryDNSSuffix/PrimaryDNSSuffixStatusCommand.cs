using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix status command.
/// </summary>
internal sealed class PrimaryDNSSuffixStatusCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="PrimaryDNSSuffixStatusCommand"/>.
    /// </summary>
    public PrimaryDNSSuffixStatusCommand()
        : base(CliStrings.Common.StatusCommandName, CliStrings.Network.DNS.Suffix.StatusCommandDescription)
        => SetAction((parseResult, cancellationToken) => StatusHandler(cancellationToken));

    private static async Task StatusHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (string.IsNullOrWhiteSpace(config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix))
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.Suffix.NotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CliStrings.Network.DNS.Suffix.Label, config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled)
            .With(config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix)
            .WriteLine();
    }
}
