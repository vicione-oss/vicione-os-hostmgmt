using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service command.
/// </summary>
internal sealed class ServiceCommand : Command
{
    private static readonly Option<string> s_nameOption = new("--name", "-n")
    {
        Description = "The name of the systemd unit.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="ServiceCommand"/>.
    /// </summary>
    public ServiceCommand() : base(
        CommandLineStrings_Systemd.ServiceCommandName,
        CommandLineStrings_Systemd.ServiceCommandDescription)
    {
        Add(new ServiceEnableCommand(s_nameOption));
        Add(new ServiceDisableCommand(s_nameOption));
        Add(new ServiceListCommand());
        Add(new ServiceStatusCommand(s_nameOption));
        Add(new ServiceStartCommand(s_nameOption));
        Add(new ServiceStopCommand(s_nameOption));
        Add(new ServiceRestartCommand(s_nameOption));
    }
}
