using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Syslog;

/// <summary>
/// Represents the syslog disable command.
/// </summary>
internal sealed class SyslogDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SyslogDisableCommand"/>.
    /// </summary>
    public SyslogDisableCommand()
        : base(CliStrings.Common.DisableCommandName, CliStrings.Syslog.DisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.Syslog.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Syslog.AlreadyDisabled);
            return;
        }

        config.Syslog.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Syslog.Disabled);
    }
}
