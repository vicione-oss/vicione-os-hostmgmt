using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the system command.
/// </summary>
internal sealed class SystemCommand : CommandBase
{
    private static readonly Option<string> s_filePathOption = new(["-f", "--filePath"])
    {
        Description = "The file path to the update file.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="SystemCommand"/>.
    /// </summary>
    public SystemCommand() : base(
        CommandLineStrings_System.SystemCommandName,
        CommandLineStrings_System.SystemCommandDescription)
    {
        Add(
            CommandLineStrings_System.SystemResetCommandName,
            CommandLineStrings_System.SystemResetCommandDescription,
            ResetHandler);

        Add(
            CommandLineStrings_System.SystemRestartCommandName,
            CommandLineStrings_System.SystemRestartCommandDescription,
            RestartHandler);

        Add(
            CommandLineStrings_System.SystemShutdownCommandName,
            CommandLineStrings_System.SystemShutdownCommandDescription,
            ShutdownHandler);

        Add(
            CommandLineStrings_System.SystemUpdateCommandName,
            CommandLineStrings_System.SystemUpdateCommandDescription,
            s_filePathOption,
            UpdateHandler);
    }

    private static async Task ResetHandler()
    {
        if (!await NamedPipeRequestHelper.ResetSystem())
        {
            SimpleOutput.WriteLine(CommandLineStrings_System.ResetFailed);
            return;
        }
    }

    private static async Task RestartHandler()
    {
        if (!await NamedPipeRequestHelper.RestartSystem())
        {
            SimpleOutput.WriteLine(CommandLineStrings_System.RestartFailed);
            return;
        }
    }

    private static async Task ShutdownHandler()
    {
        SimpleOutput.WriteLineWarning(CommandLineStrings_System.SystemCanNotRestartRemotely);
        Console.Write(CommandLineStrings_System.DoYouWantToShutdownTheSystem);

        var choice = Console.ReadLine();
        choice = string.IsNullOrWhiteSpace(choice) ? "n" : choice;

        if (!choice.Equals("y", StringComparison.OrdinalIgnoreCase))
            return;

        Console.WriteLine(CommandLineStrings_System.SystemWillShutdownNow);
        await Task.Delay(500);

        if (!await NamedPipeRequestHelper.ShutdownSystem())
        {
            SimpleOutput.WriteLine(CommandLineStrings_System.ShutdownFailed);
            return;
        }
    }

    private static async Task UpdateHandler(string filePath)
    {
        if (!await NamedPipeRequestHelper.UpdateSystem(filePath))
        {
            SimpleOutput.WriteLine(CommandLineStrings_System.UpdateFailed);
            return;
        }
    }
}
