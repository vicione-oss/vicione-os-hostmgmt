using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Suffix;

/// <summary>
/// Represents the primary DNS suffix enable command.
/// </summary>
internal sealed class DNSSuffixEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSuffixEnableCommand"/>.
    /// </summary>
    public DNSSuffixEnableCommand() : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_DNS.DNSSuffixEnableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));
    }

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.DNSSuffixEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SuffixAlreadyEnabled);
            return;
        }

        if (string.IsNullOrEmpty(config.NetworkDNSSettings.DNSSuffix))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.SuffixNotConfigured);
            return;
        }

        config.NetworkDNSSettings.DNSSuffixEnabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SuffixEnabled);
    }
}
