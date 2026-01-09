using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Suffix;

/// <summary>
/// Represents the primary DNS suffix set command.
/// </summary>
internal sealed class DNSSuffixSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSuffixSetCommand"/>.
    /// </summary>
    /// <param name="dnsSuffixOption">The primary DNS suffix option.</param>
    public DNSSuffixSetCommand(Option<string> dnsSuffixOption) : base(
        CommandLineStrings_Common.SetCommandName,
        CommandLineStrings_DNS.DNSSuffixSetCommandDescription)
    {
        Add(dnsSuffixOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return SetHandler(
                parseResult.GetRequiredValue(dnsSuffixOption),
                cancellationToken);
        });
    }

    private static async Task SetHandler(string dnsSuffix, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.DNSSuffix = dnsSuffix;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var output = new ListOutput();
        output.WithTitle(CommandLineStrings_DNS.SuffixSet);
        output.With(dnsSuffix);

        if (!config.NetworkDNSSettings.DNSSuffixEnabled)
            output.WithWarning(CommandLineStrings_DNS.SuffixCurrentlyDisabled);

        output.WriteLine();
    }
}
