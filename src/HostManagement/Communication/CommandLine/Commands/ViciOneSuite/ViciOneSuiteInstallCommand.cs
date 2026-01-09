using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite install command.
/// </summary>
internal sealed class ViciOneSuiteInstallCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteInstallCommand"/>.
    /// </summary>
    /// <param name="suiteVersionOption">The ViciOne Suite version option.</param>
    public ViciOneSuiteInstallCommand(Option<string> suiteVersionOption) : base(
        CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallCommandName,
        CommandLineStrings_ViciOneSuite.ViciOneSuiteInstallCommandDescription)
    {
        Add(suiteVersionOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return InstallHandler(
                parseResult.GetRequiredValue(suiteVersionOption),
                cancellationToken);
        });
    }

    private static async Task InstallHandler(string suiteVersion, CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.InstallViciOneSuiteVersion(suiteVersion, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_ViciOneSuite.InstalledSuccessfully, suiteVersion);
    }
}
