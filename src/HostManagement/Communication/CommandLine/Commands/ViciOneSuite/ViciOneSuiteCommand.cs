using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite command.
/// </summary>
internal sealed class ViciOneSuiteCommand : Command
{
    private static readonly Option<string> s_suiteVersion = new("--version", "-v")
    {
        Description = "The ViciOne Suite version.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteCommand"/>.
    /// </summary>
    public ViciOneSuiteCommand() : base(
        CommandLineStrings_ViciOneSuite.ViciOneSuiteCommandName,
        CommandLineStrings_ViciOneSuite.ViciOneSuiteCommandDescription)
    {
        Add(new ViciOneSuiteListVersionsCommand());
        Add(new ViciOneSuiteInstallCommand(s_suiteVersion));
        Add(new ViciOneSuiteInstallPreReleaseCommand(s_suiteVersion, hidden: true));
    }
}
