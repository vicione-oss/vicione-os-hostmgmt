using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix remove command.
/// </summary>
internal sealed class PrimaryDNSSuffixRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="PrimaryDNSSuffixRemoveCommand"/>.
    /// </summary>
    public PrimaryDNSSuffixRemoveCommand()
        : base(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_DNS.DNSSuffixRemoveCommandDescription)
        => SetAction((parseResult, cancellationToken) => RemoveHandler(cancellationToken));

    private static async Task RemoveHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var suffix = config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix;

        if (string.IsNullOrWhiteSpace(suffix))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.SuffixNotConfigured);
            return;
        }

        config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix = string.Empty;
        config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled = false;

        var output = new ListOutput();
        output.With(suffix);

        if (!await SetConfiguration(config, cancellationToken))
            return;

        output.WithTitle(CommandLineStrings_DNS.SuffixRemoved).WriteLine();
    }
}
