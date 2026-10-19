using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service command.
/// </summary>
internal sealed class ServiceCommand : Command
{
    private static readonly Argument<string> s_systemdUnitNameArgument = new("name")
    {
        Description = "The name of the systemd unit.",
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Argument<int> s_systemdUnitDelayArgument = new("delay")
    {
        Description = "The delay in seconds. (optional, must be > 0 if specified, only for start, stop, restart)",
        Arity = ArgumentArity.ZeroOrOne,
        DefaultValueFactory = _ => 0
    };

    /// <summary>
    /// Initializes a new <see cref="ServiceCommand"/>.
    /// </summary>
    public ServiceCommand() : base(CliStrings.Service.CommandName, CliStrings.Service.CommandDescription)
    {
        Add(new ServiceListCommand());
        Add(new ServiceStatusCommand(s_systemdUnitNameArgument));
        Add(new ServiceEnableCommand(s_systemdUnitNameArgument));
        Add(new ServiceDisableCommand(s_systemdUnitNameArgument));
        Add(new ServiceStartCommand(s_systemdUnitNameArgument, s_systemdUnitDelayArgument));
        Add(new ServiceStopCommand(s_systemdUnitNameArgument, s_systemdUnitDelayArgument));
        Add(new ServiceRestartCommand(s_systemdUnitNameArgument, s_systemdUnitDelayArgument));
    }
}
