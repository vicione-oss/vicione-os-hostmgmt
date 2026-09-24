using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Syslog;

/// <summary>
/// Represents the syslog enable command.
/// </summary>
internal sealed class SyslogEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SyslogEnableCommand"/>.
    /// </summary>
    public SyslogEnableCommand()
        : base(CliStrings.Common.EnableCommandName, CliStrings.Syslog.EnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.Syslog.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Syslog.AlreadyEnabled);
            return;
        }

        config.Syslog.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Syslog.Enabled);
    }
}
