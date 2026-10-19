using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="systemdUnitNameArgument">The systemd unit name argument.</param>
    /// <param name="systemdUnitDelayArgument">The systemd unit delay argument.</param>
    public ServiceRestartCommand(Argument<string> systemdUnitNameArgument, Argument<int> systemdUnitDelayArgument)
        : base(
            CliStrings.Service.RestartCommandName,
            CliStrings.Service.RestartCommandDescription + " " + CliStrings.Service.OptionalDelayCanBeSpecified)
    {
        Add(systemdUnitNameArgument);
        Add(systemdUnitDelayArgument);

        SetAction((parseResult, cancellationToken)
            => RestartHandler(
                parseResult.GetRequiredValue(systemdUnitNameArgument),
                parseResult.GetValue(systemdUnitDelayArgument),
                cancellationToken));
    }

    private static async Task RestartHandler(string name, int delay, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(name, delay, ServiceControlAction.Restart, cancellationToken);
}
