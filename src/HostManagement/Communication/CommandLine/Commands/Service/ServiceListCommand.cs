using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service (systemd unit) list command.
/// </summary>
internal sealed class ServiceListCommand : ServiceCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ServiceListCommand"/>.
    /// </summary>
    public ServiceListCommand()
        : base(
            CliStrings.Common.ListCommandName,
            CommandLineStrings_Systemd.ServiceListCommandDescription)
        => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var output = new KeyValueOutput().WithTitle(CommandLineStrings_Systemd.Units);

        config.Services.ForEach(service => output.With(new(service.Name, service.State.ToString())));

        output.WriteLine();
    }
}
