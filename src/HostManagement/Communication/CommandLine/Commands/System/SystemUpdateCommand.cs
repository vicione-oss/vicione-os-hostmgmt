using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the system update command.
/// </summary>
internal sealed class SystemUpdateCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="SystemUpdateCommand"/>.
    /// </summary>
    /// <param name="filePathArgument">The file path argument.</param>
    public SystemUpdateCommand(Argument<string> filePathArgument)
        : base(CliStrings.System.UpdateCommandName, CliStrings.System.UpdateCommandDescription)
    {
        Add(filePathArgument);

        SetAction((parseResult, cancellationToken)
            => UpdateHandler(parseResult.GetRequiredValue(filePathArgument), cancellationToken));
    }

    private static async Task UpdateHandler(string filePath, CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.UpdateSystem(filePath, cancellationToken))
        {
            SimpleOutput.WriteLine(CliStrings.System.UpdateFailed);
            return;
        }
    }
}
