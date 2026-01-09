using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite install pre-release command.
/// </summary>
internal sealed class ViciOneSuiteInstallPreReleaseCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteInstallPreReleaseCommand"/>.
    /// </summary>
    /// <param name="suitePreReleaseVersionOption">The pre-release ViciOne Suite version option.</param>
    /// <param name="hidden">Determines whether the command is hidden.</param>
    public ViciOneSuiteInstallPreReleaseCommand(
        Option<string> suitePreReleaseVersionOption,
        bool hidden = true)
        : base(
            CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallPreReleaseCommandName,
            CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallPreReleaseCommandDescription)
    {
        Hidden = hidden;

        Add(suitePreReleaseVersionOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return InstallPreReleaseHandler(
                parseResult.GetRequiredValue(suitePreReleaseVersionOption),
                cancellationToken);
        });
    }

    private static async Task InstallPreReleaseHandler(
        string suitePreReleaseVersion,
        CancellationToken cancellationToken)
    {
        var cliMessageContent = new CliMessageContent { MessageContent = suitePreReleaseVersion };

        if (!await NamedPipeRequestHelper.InstallPreReleaseViciOneSuiteVersion(cliMessageContent, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_ViciOneSuite.InstalledSuccessfully, suitePreReleaseVersion);
    }
}
