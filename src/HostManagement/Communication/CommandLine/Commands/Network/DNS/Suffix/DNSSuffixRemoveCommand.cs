using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Suffix;

/// <summary>
/// Represents the primary DNS suffix remove command.
/// </summary>
internal sealed class DNSSuffixRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSuffixRemoveCommand"/>.
    /// </summary>
    public DNSSuffixRemoveCommand() : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_DNS.DNSSuffixRemoveCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => RemoveHandler(cancellationToken));
    }

    private static async Task RemoveHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var dnsSuffix = config.NetworkDNSSettings.DNSSuffix;

        var output = new ListOutput();

        if (string.IsNullOrWhiteSpace(dnsSuffix))
        {
            output
                .WithWarning(CommandLineStrings_DNS.SuffixNotConfigured)
                .WriteLine();

            return;
        }

        config.NetworkDNSSettings.DNSSuffix = string.Empty;
        config.NetworkDNSSettings.DNSSuffixEnabled = false;

        output.With(dnsSuffix);

        if (!await SetConfiguration(config, cancellationToken))
            return;

        output
            .WithTitle(CommandLineStrings_DNS.SuffixRemoved)
            .WriteLine();
    }
}
