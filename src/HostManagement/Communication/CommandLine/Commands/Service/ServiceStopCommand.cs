using System.CommandLine;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service stop command.
/// </summary>
internal sealed class ServiceStopCommand : ServiceControlCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ServiceStopCommand"/>.
    /// </summary>
    /// <param name="systemdUnitNameOption">The systemd unit name option.</param>
    public ServiceStopCommand(Option<string> systemdUnitNameOption) : base(
        CommandLineStrings_Systemd.ServiceStopCommandName,
        CommandLineStrings_Systemd.ServiceStopCommandDescription)
    {
        Add(systemdUnitNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return StopHandler(
                parseResult.GetRequiredValue(systemdUnitNameOption),
                cancellationToken);
        });
    }

    private static async Task StopHandler(string systemdUnitName, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Stop, cancellationToken);
}
