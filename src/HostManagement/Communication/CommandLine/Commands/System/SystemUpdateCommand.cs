using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the system update command.
/// </summary>
internal sealed class SystemUpdateCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="SystemUpdateCommand"/>.
    /// </summary>
    /// <param name="filePathOption">The file path option.</param>
    public SystemUpdateCommand(Option<string> filePathOption) : base(
        CommandLineStrings_System.SystemUpdateCommandName,
        CommandLineStrings_System.SystemUpdateCommandDescription)
    {
        Add(filePathOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return UpdateHandler(
                parseResult.GetRequiredValue(filePathOption),
                cancellationToken);
        });
    }

    private static async Task UpdateHandler(string filePath, CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.UpdateSystem(filePath, cancellationToken))
        {
            SimpleOutput.WriteLine(CommandLineStrings_System.UpdateFailed);
            return;
        }
    }
}
