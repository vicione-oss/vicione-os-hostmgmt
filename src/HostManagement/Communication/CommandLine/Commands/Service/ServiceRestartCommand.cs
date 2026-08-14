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
    public ServiceRestartCommand(Argument<string> systemdUnitNameArgument)
        : base(CliStrings.Service.RestartCommandName, CliStrings.Service.RestartCommandDescription)
    {
        Add(systemdUnitNameArgument);

        SetAction((parseResult, cancellationToken)
            => RestartHandler(parseResult.GetRequiredValue(systemdUnitNameArgument), cancellationToken));
    }

    private static async Task RestartHandler(string systemdUnitName, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Restart, cancellationToken);
}
