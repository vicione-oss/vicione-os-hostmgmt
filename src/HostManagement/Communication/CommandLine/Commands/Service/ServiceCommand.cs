using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service command.
/// </summary>
internal sealed class ServiceCommand : Command
{
    private static readonly Argument<string> s_serviceNameArgument = new("name")
    {
        Description = "The name of the systemd unit.",
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="ServiceCommand"/>.
    /// </summary>
    public ServiceCommand() : base(
        CommandLineStrings_Systemd.ServiceCommandName,
        CommandLineStrings_Systemd.ServiceCommandDescription)
    {
        Add(new ServiceListCommand());
        Add(new ServiceStatusCommand(s_serviceNameArgument));
        Add(new ServiceEnableCommand(s_serviceNameArgument));
        Add(new ServiceDisableCommand(s_serviceNameArgument));
        Add(new ServiceStartCommand(s_serviceNameArgument));
        Add(new ServiceStopCommand(s_serviceNameArgument));
        Add(new ServiceRestartCommand(s_serviceNameArgument));
    }
}
