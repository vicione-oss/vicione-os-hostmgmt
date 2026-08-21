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
    public ServiceEnableCommand(Argument<string> systemdUnitNameArgument)
        : base(CliStrings.Common.EnableCommandName, CliStrings.Service.EnableCommandDescription)
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
            SimpleOutput.WriteLineError(CliStrings.Service.ConfigurationNotAllowed, systemdUnitName);
            return;
        }

        if (serviceToConfigure.State == ServiceState.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Service.AlreadyEnabled, systemdUnitName);
            return;
        }

        serviceToConfigure.State = ServiceState.Enabled;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Service.Enabled, systemdUnitName);
    }
}
