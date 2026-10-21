using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="systemdUnitDelayArgument">The systemd unit delay argument.</param>
    public ServiceStopCommand(Argument<string> systemdUnitNameArgument, Argument<int> systemdUnitDelayArgument)
        : base(
            CliStrings.Service.StopCommandName,
            CliStrings.Service.StopCommandDescription + " " + CliStrings.Service.OptionalDelayCanBeSpecified)
    {
        Add(systemdUnitNameArgument);
        Add(systemdUnitDelayArgument);

        SetAction((parseResult, cancellationToken)
            => StopHandler(
                parseResult.GetRequiredValue(systemdUnitNameArgument),
                parseResult.GetValue(systemdUnitDelayArgument),
                cancellationToken));
    }

    private static async Task StopHandler(string name, int delay, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(name, delay, ServiceControlAction.Stop, cancellationToken);
}
