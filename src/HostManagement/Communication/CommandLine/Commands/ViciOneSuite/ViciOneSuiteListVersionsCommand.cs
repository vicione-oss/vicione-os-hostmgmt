using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite list versions command.
/// </summary>
internal sealed class ViciOneSuiteListVersionsCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteListVersionsCommand"/>.
    /// </summary>
    public ViciOneSuiteListVersionsCommand() : base(
        CommandLineStrings_ViciOneSuite.ViciOneSuiteListVersionsCommandName,
        CommandLineStrings_ViciOneSuite.ViciOneSuiteListVersionsCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));
    }

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        var versions = await NamedPipeRequestHelper.GetNonPreReleaseViciOneSuiteVersions(cancellationToken);

        if (versions.Count == 0)
            return;

        new ListOutput()
            .WithTitle(CommandLineStrings_ViciOneSuite.Versions)
            .WithEach(versions.Select(CliOutputValue.Create))
            .WriteLine();
    }
}
