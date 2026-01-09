using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite command.
/// </summary>
internal sealed class ViciOneSuiteCommand : CommandBase
{
    private static readonly Option<string> s_suiteVersion = new(["-v", "--version"])
    {
        Description = "The ViciOne Suite version.",
        Arity = ArgumentArity.ExactlyOne,
        IsRequired = true,
    };

    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteCommand"/>.
    /// </summary>
    public ViciOneSuiteCommand() : base(
        CommandLineStrings_ViciOneSuite.ViciOneSuiteCommandName,
        CommandLineStrings_ViciOneSuite.ViciOneSuiteCommandDescription)
    {
        Add(
            CommandLineStrings_ViciOneSuite.ViciOneSuiteListVersionsCommandName,
            CommandLineStrings_ViciOneSuite.ViciOneSuiteListVersionsCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallCommandName,
            CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallCommandDescription,
            s_suiteVersion,
            InstallHandler);

        Add(
            CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallPreReleaseCommandName,
            CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallPreReleaseCommandDescription,
            s_suiteVersion,
            InstallPreReleaseHandler,
            isHidden: true);
    }

    private static async Task ListHandler()
    {
        var versions = await NamedPipeRequestHelper.GetNonPreReleaseViciOneSuiteVersions();

        if (versions.Count == 0)
            return;

        new ListOutput()
            .WithTitle(CommandLineStrings_ViciOneSuite.Versions)
            .WithEach(versions.Select(CliOutputValue.Create))
            .WriteLine();
    }

    private static async Task InstallHandler(string suiteVersion)
    {
        if (!await NamedPipeRequestHelper.InstallViciOneSuiteVersion(suiteVersion))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_ViciOneSuite.InstalledSuccessfully, suiteVersion);
    }

    private static async Task InstallPreReleaseHandler(string suiteVersion)
    {
        var cliMessageContent = new CliMessageContent { MessageContent = suiteVersion };

        if (!await NamedPipeRequestHelper.InstallPreReleaseViciOneSuiteVersion(cliMessageContent))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_ViciOneSuite.InstalledSuccessfully, suiteVersion);
    }
}
