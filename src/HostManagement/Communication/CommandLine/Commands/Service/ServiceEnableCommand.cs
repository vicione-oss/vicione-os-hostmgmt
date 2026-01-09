using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service (systemd unit) enable command.
/// </summary>
internal sealed class ServiceEnableCommand : ServiceCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ServiceEnableCommand"/>.
    /// </summary>
    /// <param name="systemdUnitNameOption">The systemd unit name option.</param>
    public ServiceEnableCommand(Option<string> systemdUnitNameOption) : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_Systemd.ServiceEnableCommandDescription)
    {
        Add(systemdUnitNameOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return EnableHandler(
                parseResult.GetRequiredValue(systemdUnitNameOption),
                cancellationToken);
        });
    }

    private static async Task EnableHandler(string systemdUnitName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var serviceToConfigure = GetServiceDetailToConfigure(config, systemdUnitName);

        if (serviceToConfigure is null)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_Systemd.UnitConfigurationNotAllowed, systemdUnitName);
            return;
        }

        if (serviceToConfigure.State == ServiceState.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Systemd.UnitAlreadyEnabled, systemdUnitName);
            return;
        }

        serviceToConfigure.State = ServiceState.Enabled;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Systemd.UnitEnabled, systemdUnitName);
    }
}
