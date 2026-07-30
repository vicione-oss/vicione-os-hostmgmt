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
    public PrimaryDNSSuffixRemoveCommand() : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_DNS.DNSSuffixRemoveCommandDescription)
            => SetAction((parseResult, cancellationToken) => RemoveHandler(cancellationToken));

    private static async Task RemoveHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var suffix = config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix;

        var output = new ListOutput();

        if (string.IsNullOrWhiteSpace(suffix))
        {
            output
                .WithWarning(CommandLineStrings_DNS.SuffixNotConfigured)
                .WriteLine();

            return;
        }

        config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix = string.Empty;
        config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled = false;

        output.With(suffix);

        if (!await SetConfiguration(config, cancellationToken))
            return;

        output
            .WithTitle(CommandLineStrings_DNS.SuffixRemoved)
            .WriteLine();
    }
}
