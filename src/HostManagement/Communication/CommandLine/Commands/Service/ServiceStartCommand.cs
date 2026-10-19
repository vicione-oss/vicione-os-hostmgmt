using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="systemdUnitDelayArgument">The systemd unit delay argument.</param>
    public ServiceStartCommand(Argument<string> systemdUnitNameArgument, Argument<int> systemdUnitDelayArgument)
        : base(
            CliStrings.Service.StartCommandName,
            CliStrings.Service.StartCommandDescription + " " + CliStrings.Service.OptionalDelayCanBeSpecified)
    {
        Add(systemdUnitNameArgument);
        Add(systemdUnitDelayArgument);

        SetAction((parseResult, cancellationToken)
            => StartHandler(
                parseResult.GetRequiredValue(systemdUnitNameArgument),
                parseResult.GetValue(systemdUnitDelayArgument),
                cancellationToken));
    }

    private static async Task StartHandler(string name, int delay, CancellationToken cancellationToken)
        => await ControlSystemdUnitHandler(name, delay, ServiceControlAction.Start, cancellationToken);
}
