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
    /// <param name="delayArgument">The delay argument.</param>
    public SystemShutdownCommand(Argument<int> delayArgument) : base(
        CommandLineStrings_System.SystemShutdownCommandName,
        CommandLineStrings_System.SystemShutdownCommandDescription +
        " " +
        CommandLineStrings_System.OptionalDelayCanBeSpecified)
    {
        Add(delayArgument);

        SetAction((parseResult, cancellationToken)
            => ShutdownHandler(parseResult.GetValue(delayArgument), cancellationToken));
    }

    private static async Task ShutdownHandler(int delayInSeconds, CancellationToken cancellationToken)
    {
        SimpleOutput.WriteLineWarning(CommandLineStrings_System.SystemCanNotRestartRemotely);
        Console.Write(CommandLineStrings_System.DoYouWantToShutdownTheSystem);

        var choice = Console.ReadLine();
        choice = string.IsNullOrWhiteSpace(choice) ? "n" : choice;

        if (!choice.Equals("y", StringComparison.OrdinalIgnoreCase))
            return;

        if (await NamedPipeRequestHelper.ShutdownSystem(delayInSeconds, cancellationToken) is string message)
            SimpleOutput.WriteLine(message);
    }
}
