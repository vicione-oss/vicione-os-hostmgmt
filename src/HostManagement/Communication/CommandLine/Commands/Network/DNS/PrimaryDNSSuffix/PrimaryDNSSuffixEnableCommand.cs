using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix enable command.
/// </summary>
internal sealed class PrimaryDNSSuffixEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="PrimaryDNSSuffixEnableCommand"/>.
    /// </summary>
    public PrimaryDNSSuffixEnableCommand()
        : base(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_DNS.DNSSuffixEnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SuffixAlreadyEnabled);
            return;
        }

        if (string.IsNullOrEmpty(config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.SuffixNotConfigured);
            return;
        }

        config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SuffixEnabled);
    }
}
