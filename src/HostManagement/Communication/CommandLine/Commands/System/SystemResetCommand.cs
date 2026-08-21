using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the system reset command.
/// </summary>
internal sealed class SystemResetCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="SystemResetCommand"/>.
    /// </summary>
    public SystemResetCommand() : base(CliStrings.System.ResetCommandName, CliStrings.System.ResetCommandDescription)
        => SetAction((parseResult, cancellationToken) => ResetHandler(cancellationToken));

    private static async Task ResetHandler(CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.ResetSystem(cancellationToken))
        {
            SimpleOutput.WriteLine(CliStrings.System.ResetFailed);
            return;
        }
    }
}
