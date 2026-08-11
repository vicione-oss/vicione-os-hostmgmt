using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service (systemd unit) status command.
/// </summary>
internal sealed class ServiceStatusCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="ServiceStatusCommand"/>.
    /// </summary>
    /// <param name="systemdUnitNameArgument">The systemd unit name argument.</param>
    public ServiceStatusCommand(Argument<string> systemdUnitNameArgument) : base(
        CommandLineStrings_Common.StatusCommandName,
        CommandLineStrings_Systemd.ServiceStatusCommandDescription)
    {
        Add(systemdUnitNameArgument);

        SetAction((parseResult, cancellationToken) =>
        {
            return StatusHandler(
                parseResult.GetRequiredValue(systemdUnitNameArgument),
                cancellationToken);
        });
    }

    private static async Task StatusHandler(string systemdUnitName, CancellationToken cancellationToken)
    {
        if (await NamedPipeRequestHelper.GetSystemdUnitActiveState(systemdUnitName, cancellationToken) is string activeState)
            SimpleOutput.WriteLine(activeState);
    }
}
