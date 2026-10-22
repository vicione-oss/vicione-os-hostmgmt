using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts.System;


namespace HostManagement.Communication.CommandLine.Commands.System;

/// <summary>
/// Represents the Install signed Debian package command.
/// </summary>
internal sealed class InstallSignedDebianPackageCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="InstallSignedDebianPackageCommand"/>.
    /// </summary>
    /// <param name="filePathOption">The file path option.</param>
    /// <param name="signatureFilePathOption">The signature file path option.</param>
    public InstallSignedDebianPackageCommand(Option<string> filePathOption, Option<string> signatureFilePathOption)
        : base(CliStrings.System.InstallPackageCommandName, CliStrings.System.InstallPackageCommandDescription)
    {
        Add(filePathOption);
        Add(signatureFilePathOption);

        SetAction((parseResult, cancellationToken)
            => InstallHandler(
                parseResult.GetRequiredValue(filePathOption),
                parseResult.GetRequiredValue(signatureFilePathOption),
                cancellationToken));
    }

    private static async Task InstallHandler(
        string filePath,
        string signatureFilePath,
        CancellationToken cancellationToken)
    {
        var signedDebianPackage = new SignedDebianPackage
        {
            FilePath = filePath,
            SignatureFilePath = signatureFilePath
        };

        if (!await NamedPipeRequestHelper.InstallSignedDebianPackage(signedDebianPackage, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.System.InstallPackageSuccessful, filePath);
    }
}
