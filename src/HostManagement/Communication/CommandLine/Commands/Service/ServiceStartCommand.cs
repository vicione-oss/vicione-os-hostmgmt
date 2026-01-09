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
    /// <param name="systemdUnitNameOption">The systemd unit name option.</param>
    public ServiceStartCommand(Option<string> systemdUnitNameOption) : base(
        CommandLineStrings_Systemd.ServiceStartCommandName,
        CommandLineStrings_Systemd.ServiceStartCommandDescription)
    {
        Add(systemdUnitNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return StartHandler(
                parseResult.GetRequiredValue(systemdUnitNameOption),
                cancellationToken);
        });
    }

    private static async Task StartHandler(string systemdUnitName, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Start, cancellationToken);
}
