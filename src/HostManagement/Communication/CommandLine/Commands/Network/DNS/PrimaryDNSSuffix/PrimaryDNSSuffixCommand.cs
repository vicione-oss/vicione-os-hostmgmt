using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix command.
/// </summary>
internal sealed class PrimaryDNSSuffixCommand : Command
{
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
        Add(new PrimaryDNSSuffixSetCommand());
        Add(new PrimaryDNSSuffixRemoveCommand());
    }
}
