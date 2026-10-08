using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Syslog;

/// <summary>
/// Represents the syslog command.
/// </summary>
internal sealed class SyslogCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="SyslogCommand"/>.
    /// </summary>
    public SyslogCommand() : base(CliStrings.Syslog.CommandName, CliStrings.Syslog.CommandDescription)
    {
        Add(new SyslogStatusCommand());
        Add(new SyslogEnableCommand());
        Add(new SyslogDisableCommand());
        Add(new SyslogSetCommand());
        Add(new SyslogClearCommand());
    }
}
