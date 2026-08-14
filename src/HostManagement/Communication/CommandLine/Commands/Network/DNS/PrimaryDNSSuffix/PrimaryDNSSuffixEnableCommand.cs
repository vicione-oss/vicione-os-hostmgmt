using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.DNS.Suffix.EnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.Suffix.AlreadyEnabled);
            return;
        }

        if (string.IsNullOrEmpty(config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix))
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.DNS.Suffix.NotConfigured);
            return;
        }

        config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.Suffix.Enabled);
    }
}
