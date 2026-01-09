using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Suffix;

/// <summary>
/// Represents the primary DNS suffix command.
/// </summary>
internal sealed class DNSSuffixCommand : Command
{
    private static readonly Option<string> s_suffixOption = new("--suffix", "-s")
    {
        Description = "The primary DNS suffix.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="DNSSuffixCommand"/>.
    /// </summary>
    public DNSSuffixCommand() : base(
        CommandLineStrings_DNS.DNSSuffixCommandName,
        CommandLineStrings_DNS.DNSSuffixCommandDescription)
    {
        Add(new DNSSuffixEnableCommand());
        Add(new DNSSuffixDisableCommand());
        Add(new DNSSuffixStatusCommand());
        Add(new DNSSuffixSetCommand(s_suffixOption));
        Add(new DNSSuffixRemoveCommand());
    }
}
