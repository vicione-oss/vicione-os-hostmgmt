using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.NTP;

/// <summary>
/// Represents the NTP command.
/// </summary>
internal sealed class NTPCommand : Command
{
    private static readonly Option<string> s_oldNTPServerOption = new("--old-ntp-server", "-o")
    {
        Description = "The old NTP server.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Option<string> s_newNTPServerOption = new("--new-ntp-server", "-n")
    {
        Description = "The new NTP server.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="NTPCommand"/>.
    /// </summary>
    public NTPCommand() : base(
        CommandLineStrings_NTP.NTPCommandName,
        CommandLineStrings_NTP.NTPCommandDescription)
    {
        Add(new NTPEnableCommand());
        Add(new NTPDisableCommand());
        Add(new NTPListCommand());
        Add(new NTPAddCommand(CommonOptions.s_itemsOption));
        Add(new NTPRemoveCommand(CommonOptions.s_itemsOption));
        Add(new NTPSetCommand(CommonOptions.s_itemsOption));
        Add(new NTPClearCommand());
        Add(new NTPModifyCommand(s_oldNTPServerOption, s_newNTPServerOption));
    }
}
