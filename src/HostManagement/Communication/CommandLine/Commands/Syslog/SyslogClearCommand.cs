using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.Communication.CommandLine.Commands.Syslog;

/// <summary>
/// Represents the syslog clear command.
/// </summary>
internal sealed class SyslogClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SyslogClearCommand"/>.
    /// </summary>
    public SyslogClearCommand()
        : base(CliStrings.Common.ClearCommandName, CliStrings.Syslog.ClearCommandDescription)
        => SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.Syslog = new SyslogSettings();

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Syslog.Cleared);
    }
}
