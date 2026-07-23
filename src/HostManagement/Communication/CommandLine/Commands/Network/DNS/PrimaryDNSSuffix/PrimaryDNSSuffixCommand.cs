using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix command.
/// </summary>
internal sealed class PrimaryDNSSuffixCommand : Command
{
    private static readonly Option<string> s_suffixOption = new("--suffix", "-s")
    {
        Description = "The primary DNS suffix.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="PrimaryDNSSuffixCommand"/>.
    /// </summary>
    public PrimaryDNSSuffixCommand() : base(
        CommandLineStrings_DNS.DNSSuffixCommandName,
        CommandLineStrings_DNS.DNSSuffixCommandDescription)
    {
        Add(new PrimaryDNSSuffixEnableCommand());
        Add(new PrimaryDNSSuffixDisableCommand());
        Add(new PrimaryDNSSuffixStatusCommand());
        Add(new PrimaryDNSSuffixSetCommand(s_suffixOption));
        Add(new PrimaryDNSSuffixRemoveCommand());
    }
}
