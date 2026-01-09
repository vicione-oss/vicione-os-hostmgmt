using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the system shutdown command.
/// </summary>
internal sealed class SystemShutdownCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="SystemShutdownCommand"/>.
    /// </summary>
    public SystemShutdownCommand() : base(
        CommandLineStrings_System.SystemShutdownCommandName,
        CommandLineStrings_System.SystemShutdownCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ShutdownHandler(cancellationToken));
    }

    private static async Task ShutdownHandler(CancellationToken cancellationToken)
    {
        SimpleOutput.WriteLineWarning(CommandLineStrings_System.SystemCanNotRestartRemotely);
        Console.Write(CommandLineStrings_System.DoYouWantToShutdownTheSystem);

        var choice = Console.ReadLine();
        choice = string.IsNullOrWhiteSpace(choice) ? "n" : choice;

        if (!choice.Equals("y", StringComparison.OrdinalIgnoreCase))
            return;

        Console.WriteLine(CommandLineStrings_System.SystemWillShutdownNow);
        await Task.Delay(500, cancellationToken);

        if (!await NamedPipeRequestHelper.ShutdownSystem(cancellationToken))
        {
            SimpleOutput.WriteLine(CommandLineStrings_System.ShutdownFailed);
            return;
        }
    }
}
