using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the system command.
/// </summary>
internal sealed class SystemCommand : Command
{
    private static readonly Argument<int> s_delayArgument = new("delay")
    {
        Description = "The delay in seconds.",
        Arity = ArgumentArity.ZeroOrOne
    };

    private static readonly Option<string> s_filePathOption = new("--filePath", "-f")
    {
        Description = "The file path.",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };

    private static readonly Option<string> s_signatureFilePathOption = new("--signaturePath", "-s")
    {
        Description = "The file path to the package signature.",
        Arity = ArgumentArity.ExactlyOne,
        Required = true,
    };

    /// <summary>
    /// Initializes a new <see cref="SystemCommand"/>.
    /// </summary>
    public SystemCommand() : base(
        CommandLineStrings_System.SystemCommandName,
        CommandLineStrings_System.SystemCommandDescription)
    {
        Add(new SystemResetCommand());
        Add(new SystemRestartCommand(s_delayArgument));
        Add(new SystemShutdownCommand(s_delayArgument));
        Add(new SystemUpdateCommand(s_filePathOption));
        Add(new InstallSignedDebianPackageCommand(s_filePathOption, s_signatureFilePathOption));
    }
}
