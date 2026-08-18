using System.CommandLine;
using System.Text.Json;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts.System;
using SharedJsonContext = HostManagement.Shared.Contracts.SourceGenerationContext;


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
    public InstallSignedDebianPackageCommand(
        Option<string> filePathOption,
        Option<string> signatureFilePathOption)
        : base(
            CommandLineStrings_System.InstallPackageCommandName,
            CommandLineStrings_System.InstallPackageCommandDescription)
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
        var messageContent = JsonSerializer.Serialize(
            new SignedDebianPackage
            {
                FilePath = filePath,
                SignatureFilePath = signatureFilePath
            },
            SharedJsonContext.Default.SignedDebianPackage);

        if (!await NamedPipeRequestHelper.InstallSignedDebianPackage(messageContent, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_System.InstallPackageSuccessfull, filePath);
    }
}
