using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the abstract base class for a service control command (e.g. start, stop, restart).
/// </summary>
internal abstract class ServiceControlCommandBase(string name, string description)
    : ServiceCommandBase(name, description)
{
    private protected static async Task ControlSystemdUnitHandler(
        string name,
        int delay,
        ServiceControlAction action,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            SimpleOutput.WriteLineError(CliStrings.Service.Empty, name);
            return;
        }

        var normalizedName = GetSystemdUnitWithUnitType(name);

        var request = new ServiceControlRequest
        {
            Name = normalizedName,
            DelayInSeconds = delay
        };

        if (!await NamedPipeRequestHelper.ControlSystemdUnit(request, action, cancellationToken))
            return;

        var actionString = action.ToString().ToLowerInvariant();

        if (delay > 0)
            SimpleOutput.WriteLine(CliStrings.Service.WillBeControlled, normalizedName, actionString, delay);
        else
            SimpleOutput.WriteLine(CliStrings.Service.Controlled, normalizedName, actionString);
    }
}
