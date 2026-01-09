using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

/// <summary>
/// Represents the <see cref="ViciOneSuiteCommandGroup"/>.
/// </summary>
internal static class ViciOneSuiteCommandGroup
{
    private static readonly Option<string> s_suiteVersion = new(["-v", "--version"])
    {
        Description = "The ViciOne Suite version.",
        Arity = ArgumentArity.ExactlyOne,
        IsRequired = true,
    };

    private static readonly Command s_listSuiteVersionsCommand = CommandHelper.ConstructCommand(
        "list-versions",
        "List all compatible ViciOne Suite versions.",
        ListHandler);

    private static readonly Command s_installSuiteVersionCommand = CommandHelper.ConstructCommand(
        "install",
        "Install the specified ViciOne Suite version.",
        s_suiteVersion,
        InstallHandler);

    private static readonly Command s_installPreReleaseSuiteVersionCommand = CommandHelper.ConstructCommand(
        "install-pre-release",
        "Install the specified pre-release ViciOne Suite version.",
        s_suiteVersion,
        InstallPreReleaseHandler,
        isHidden: true);

    /// <summary>
    /// Adds the suite command group.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddSuiteCommandGroup(this Command parentCommand)
    {
        var suiteCommand = parentCommand.AddCommand(
            "suite",
            "Install/Update ViciOne Suite.");

        suiteCommand.AddCommand(s_listSuiteVersionsCommand);
        suiteCommand.AddCommand(s_installSuiteVersionCommand);
        suiteCommand.AddCommand(s_installPreReleaseSuiteVersionCommand);
    }

    private static async Task ListHandler()
    {
        var versions = await NamedPipeRequestHelper.GetNonPreReleaseViciOneSuiteVersions();

        if (versions.Count == 0)
            return;

        new ListOutput()
            .WithTitle(CommandLineStrings.ViciOneSuiteVersions)
            .WithEach(versions.Select(CliOutputValue.Create))
            .WriteLine();
    }

    private static async Task InstallHandler(string suiteVersion)
    {
        if (!await NamedPipeRequestHelper.InstallViciOneSuiteVersion(suiteVersion))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.ViciOneSuiteInstalledSuccessfully, suiteVersion);
    }

    private static async Task InstallPreReleaseHandler(string suiteVersion)
    {
        var cliMessageContent = new CliMessageContent { MessageContent = suiteVersion };

        if (!await NamedPipeRequestHelper.InstallPreReleaseViciOneSuiteVersion(cliMessageContent))
            return;

        SimpleOutput.WriteLine(CommandLineStrings.ViciOneSuiteInstalledSuccessfully, suiteVersion);
    }
}
