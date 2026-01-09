using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the system restart command.
/// </summary>
internal sealed class SystemRestartCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="SystemRestartCommand"/>.
    /// </summary>
    public SystemRestartCommand() : base(
        CommandLineStrings_System.SystemRestartCommandName,
        CommandLineStrings_System.SystemRestartCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => RestartHandler(cancellationToken));
    }

    private static async Task RestartHandler(CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.RestartSystem(cancellationToken))
        {
            SimpleOutput.WriteLine(CommandLineStrings_System.RestartFailed);
            return;
        }
    }
}
