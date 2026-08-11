using System.CommandLine;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service start command.
/// </summary>
internal sealed class ServiceStartCommand : ServiceControlCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ServiceStartCommand"/>.
    /// </summary>
    /// <param name="systemdUnitNameArgument">The systemd unit name argument.</param>
    public ServiceStartCommand(Argument<string> systemdUnitNameArgument) : base(
        CommandLineStrings_Systemd.ServiceStartCommandName,
        CommandLineStrings_Systemd.ServiceStartCommandDescription)
    {
        Add(systemdUnitNameArgument);

        SetAction((parseResult, cancellationToken)
            => StartHandler(parseResult.GetRequiredValue(systemdUnitNameArgument), cancellationToken));
    }

    private static async Task StartHandler(string systemdUnitName, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Start, cancellationToken);
}
