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
    /// <param name="delayArgument">The delay argument.</param>
    public SystemRestartCommand(Argument<int> delayArgument) : base(
        CommandLineStrings_System.SystemRestartCommandName,
        CommandLineStrings_System.SystemRestartCommandDescription +
        " " +
        CommandLineStrings_System.OptionalDelayCanBeSpecified)
    {
        Add(delayArgument);

        SetAction((parseResult, cancellationToken) =>
        {
            return RestartHandler(
                parseResult.GetValue(delayArgument),
                cancellationToken);
        });
    }

    private static async Task RestartHandler(int delayInSeconds, CancellationToken cancellationToken)
    {
        if (await NamedPipeRequestHelper.RestartSystem(delayInSeconds, cancellationToken) is string message)
            SimpleOutput.WriteLine(message);
    }
}
