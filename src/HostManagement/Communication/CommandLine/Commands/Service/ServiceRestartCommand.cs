using System.CommandLine;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service restart command.
/// </summary>
internal sealed class ServiceRestartCommand : ServiceControlCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ServiceRestartCommand"/>.
    /// </summary>
    /// <param name="systemdUnitNameOption">The systemd unit name option.</param>
    public ServiceRestartCommand(Option<string> systemdUnitNameOption) : base(
        CommandLineStrings_Systemd.ServiceRestartCommandName,
        CommandLineStrings_Systemd.ServiceRestartCommandDescription)
    {
        Add(systemdUnitNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return RestartHandler(
                parseResult.GetRequiredValue(systemdUnitNameOption),
                cancellationToken);
        });
    }

    private static async Task RestartHandler(string systemdUnitName, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Restart, cancellationToken);
}
