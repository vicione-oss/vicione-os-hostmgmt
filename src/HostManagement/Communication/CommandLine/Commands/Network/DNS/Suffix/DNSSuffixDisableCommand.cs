using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Suffix;

/// <summary>
/// Represents the primary DNS suffix disable command.
/// </summary>
internal sealed class DNSSuffixDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSuffixDisableCommand"/>.
    /// </summary>
    public DNSSuffixDisableCommand() : base(
        CommandLineStrings_Common.DisableCommandName,
        CommandLineStrings_DNS.DNSSuffixDisableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));
    }

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.DNSSuffixEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SuffixAlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.DNSSuffixEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SuffixDisabled);
    }
}
