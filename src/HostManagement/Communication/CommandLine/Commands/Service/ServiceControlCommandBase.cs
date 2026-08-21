using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the abstract base class for a service control command (e.g. start, stop, restart).
/// </summary>
internal abstract class ServiceControlCommandBase(string name, string description)
    : ServiceCommandBase(name, description)
{
    private protected static async Task ControlSystemdUnitHandler(
        string systemdUnitName,
        ServiceControlAction controlAction,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(systemdUnitName))
        {
            SimpleOutput.WriteLineError(CliStrings.Service.Empty, systemdUnitName);
            return;
        }

        var normalizedUnitName = GetSystemdUnitWithUnitType(systemdUnitName);

        if (!await NamedPipeRequestHelper.ControlSystemdUnit(normalizedUnitName, controlAction, cancellationToken))
            return;

        var successMessage = controlAction switch
        {
            ServiceControlAction.Start => CliStrings.Service.Started,
            ServiceControlAction.Stop => CliStrings.Service.Stopped,
            ServiceControlAction.Restart => CliStrings.Service.Restarted,
            _ => throw new ArgumentOutOfRangeException(nameof(controlAction), controlAction, null)
        };

        SimpleOutput.WriteLine(successMessage, systemdUnitName);
    }
}
