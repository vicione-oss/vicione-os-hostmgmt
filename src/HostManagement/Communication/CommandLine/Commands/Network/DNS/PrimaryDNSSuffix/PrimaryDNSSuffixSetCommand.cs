using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix set command.
/// </summary>
internal sealed class PrimaryDNSSuffixSetCommand : ConfigCommandBase
{
    private static readonly Argument<string> s_suffixArgument = new("suffix")
    {
        Description = "The new primary DNS suffix.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="PrimaryDNSSuffixSetCommand"/>.
    /// </summary>
    public PrimaryDNSSuffixSetCommand()
        : base(CliStrings.Common.SetCommandName, CliStrings.Network.DNS.Suffix.SetCommandDescription)
    {
        Add(s_suffixArgument);

        SetAction((parseResult, cancellationToken)
            => SetHandler(parseResult.GetRequiredValue(s_suffixArgument), cancellationToken));
    }

    private static async Task SetHandler(string suffix, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix = suffix;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var output = new ListOutput();
        output.WithTitle(CliStrings.Network.DNS.Suffix.Set);
        output.With(suffix);

        if (!config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled)
            output.WithWarning(CliStrings.Network.DNS.Suffix.CurrentlyDisabled);

        output.WriteLine();
    }
}
