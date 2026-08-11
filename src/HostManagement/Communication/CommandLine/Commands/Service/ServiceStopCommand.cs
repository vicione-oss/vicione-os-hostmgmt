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
    /// <param name="systemdUnitNameArgument">The systemd unit name argument.</param>
    public ServiceStopCommand(Argument<string> systemdUnitNameArgument) : base(
        CommandLineStrings_Systemd.ServiceStopCommandName,
        CommandLineStrings_Systemd.ServiceStopCommandDescription)
    {
        Add(systemdUnitNameArgument);

        SetAction((parseResult, cancellationToken)
            => StopHandler(parseResult.GetRequiredValue(systemdUnitNameArgument), cancellationToken));
    }

    private static async Task StopHandler(string systemdUnitName, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Stop, cancellationToken);
}
