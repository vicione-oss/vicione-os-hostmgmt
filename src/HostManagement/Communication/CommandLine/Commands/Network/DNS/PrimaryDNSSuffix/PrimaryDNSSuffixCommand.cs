using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.PrimaryDNSSuffix;

/// <summary>
/// Represents the primary DNS suffix command.
/// </summary>
internal sealed class PrimaryDNSSuffixCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="PrimaryDNSSuffixCommand"/>.
    /// </summary>
    public PrimaryDNSSuffixCommand()
        : base(CliStrings.Network.DNS.Suffix.CommandName, CliStrings.Network.DNS.Suffix.CommandDescription)
    {
        Add(new PrimaryDNSSuffixStatusCommand());
        Add(new PrimaryDNSSuffixEnableCommand());
        Add(new PrimaryDNSSuffixDisableCommand());
        Add(new PrimaryDNSSuffixSetCommand());
        Add(new PrimaryDNSSuffixRemoveCommand());
    }
}
