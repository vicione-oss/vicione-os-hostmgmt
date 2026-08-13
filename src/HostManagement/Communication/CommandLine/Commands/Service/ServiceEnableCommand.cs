using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    /// <param name="systemdUnitNameArgument">The systemd unit name argument.</param>
    public ServiceEnableCommand(Argument<string> systemdUnitNameArgument) : base(
        CliStrings.Common.EnableCommandName,
        CommandLineStrings_Systemd.ServiceEnableCommandDescription)
    {
        Add(systemdUnitNameArgument);

        SetAction((parseResult, cancellationToken)
            => EnableHandler(parseResult.GetRequiredValue(systemdUnitNameArgument), cancellationToken));
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
