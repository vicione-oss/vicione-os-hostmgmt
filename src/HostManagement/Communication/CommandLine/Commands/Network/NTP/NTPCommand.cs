using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP command.
/// </summary>
internal sealed class NTPCommand : Command
{
    private static readonly Argument<string[]> s_ntpServersArgument = new("servers")
    {
        Description = "The list of NTP servers.",
        Arity = ArgumentArity.OneOrMore
    };

    private static readonly Option<string> s_oldNTPServerOption = new("--old-server", "-o")
    {
        Description = "The old NTP server.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string> s_newNTPServerOption = new("--new-server", "-n")
    {
        Description = "The new NTP server.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="NTPCommand"/>.
    /// </summary>
    public NTPCommand() : base(CliStrings.Network.NTP.CommandName, CliStrings.Network.NTP.CommandDescription)
    {
        Add(new NTPStatusCommand());
        Add(new NTPListCommand());
        Add(new NTPEnableCommand());
        Add(new NTPDisableCommand());
        Add(new NTPAddCommand(s_ntpServersArgument));
        Add(new NTPRemoveCommand(s_ntpServersArgument));
        Add(new NTPSetCommand(s_ntpServersArgument));
        Add(new NTPClearCommand());
        Add(new NTPModifyCommand(s_oldNTPServerOption, s_newNTPServerOption));
    }
}
