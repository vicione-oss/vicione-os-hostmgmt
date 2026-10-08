using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.Communication.CommandLine.Commands.Syslog;

/// <summary>
/// Represents the syslog status command.
/// </summary>
internal sealed class SyslogStatusCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SyslogStatusCommand"/>.
    /// </summary>
    public SyslogStatusCommand()
        : base(CliStrings.Common.StatusCommandName, CliStrings.Syslog.StatusCommandDescription)
        => SetAction((parseResult, cancellationToken) => StatusHandler(cancellationToken));

    private static async Task StatusHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var syslog = config.Syslog;

        var output = new KeyValueOutput()
            .WithTitle(CliStrings.Syslog.Label, syslog.Enabled)
            .With(new(CliStrings.Syslog.Address, syslog.Address))
            .With(new(CliStrings.Syslog.TransportProtocol, syslog.TransportProtocol.ToString().ToLowerInvariant()))
            .With(new(CliStrings.Syslog.LogFormat, syslog.LogFormat.ToString().ToLowerInvariant()))
            .With(new(CliStrings.Syslog.ConnectionRetrySeconds, syslog.ConnectionRetrySeconds));

        var facilitiesOutput = output
            .AddListOutput(1)
            .WithTitle(CliStrings.Syslog.ExcludeSyslogFacilities);

        if (syslog.ExcludeSyslogFacilities.Count == 0)
        {
            facilitiesOutput.With(CliOutputBase.EmptyValue);
        }
        else
        {
            syslog.ExcludeSyslogFacilities.ForEach(facility =>
            {
                if (facility == SyslogFacility.SolarisCron)
                    facilitiesOutput.With("solaris-cron");
                else
                    facilitiesOutput.With(facility.ToString().ToLowerInvariant());
            });
        }

        var levelsOutput = output
            .AddListOutput(1)
            .WithTitle(CliStrings.Syslog.ExcludeSyslogLevels);

        if (syslog.ExcludeSyslogLevels.Count == 0)
            levelsOutput.With(CliOutputBase.EmptyValue);
        else
            syslog.ExcludeSyslogLevels.ForEach(level => levelsOutput.With(level.ToString().ToLowerInvariant()));

        output.WriteLine();
    }
}
