using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix disable command.
/// </summary>
internal sealed class PrimaryDNSSuffixDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="PrimaryDNSSuffixDisableCommand"/>.
    /// </summary>
    public PrimaryDNSSuffixDisableCommand()
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.DNS.Suffix.DisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.Suffix.AlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.Suffix.Disabled);
    }
}
