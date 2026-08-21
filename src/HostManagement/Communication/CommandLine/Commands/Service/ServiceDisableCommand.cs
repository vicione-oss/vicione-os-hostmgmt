using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service (systemd unit) disable command.
/// </summary>
internal sealed class ServiceDisableCommand : ServiceCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ServiceDisableCommand"/>.
    /// </summary>
    /// <param name="systemdUnitNameArgument">The systemd unit name argument.</param>
    public ServiceDisableCommand(Argument<string> systemdUnitNameArgument)
        : base(CliStrings.Common.DisableCommandName, CliStrings.Service.DisableCommandDescription)
    {
        Add(systemdUnitNameArgument);

        SetAction((parseResult, cancellationToken)
            => DisableHandler(parseResult.GetRequiredValue(systemdUnitNameArgument), cancellationToken));
    }

    private static async Task DisableHandler(string systemdUnitName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var serviceToConfigure = GetServiceDetailToConfigure(config, systemdUnitName);

        if (serviceToConfigure is null)
        {
            SimpleOutput.WriteLineError(CliStrings.Service.ConfigurationNotAllowed, systemdUnitName);
            return;
        }

        if (serviceToConfigure.State == ServiceState.Disabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Service.AlreadyDisabled, systemdUnitName);
            return;
        }

        serviceToConfigure.State = ServiceState.Disabled;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Service.Disabled, systemdUnitName);
    }
}
