using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the <see cref="SystemCommandGroupProvider"/>.
/// </summary>
internal static class SystemCommandGroupProvider
{
    private static readonly Command s_resetCommand = CommandHelper.ConstructCommand(
        "reset",
        "Reset the system.",
        ResetHandler);
    private static readonly Command s_restartCommand = CommandHelper.ConstructCommand(
        "restart",
        "Restart the system.",
        RestartHandler);

    /// <summary>
    /// Adds the system command group.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddSystemCommandGroup(this Command parentCommand)
    {
        var systemCommand = parentCommand.AddCommand(
            "system",
            "Execute system wide commands.");

        systemCommand.AddCommand(s_resetCommand);
        systemCommand.AddCommand(s_restartCommand);
    }

    private static async Task ResetHandler()
    {
        if (!await NamedPipeRequestHelper.ResetSystem())
        {
            SimpleOutput.WriteLine(CommandLineStrings.SystemResetFailed);
            return;
        }
    }

    private static async Task RestartHandler()
    {
        if (!await NamedPipeRequestHelper.RestartSystem())
        {
            SimpleOutput.WriteLine(CommandLineStrings.SystemRestartFailed);
            return;
        }
    }
}
