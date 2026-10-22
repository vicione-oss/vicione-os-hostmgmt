using System.Text.Json;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;
using HostManagement.Shared.Contracts.System;
using Microsoft.Extensions.Logging;
using SharedJsonContext = HostManagement.Shared.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="InstallSignedDebianPackageRequestHandler"/>.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class InstallSignedDebianPackageRequestHandler(ILogger logger) : RequestHandler<SystemControlResult>
{
    /// <summary>
    /// The topic of the <see cref="InstallSignedDebianPackageRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.InstallSignedDebianPackage;

    /// <summary>
    /// Handles the install signed Debian package request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var signedDebianPackage = JsonSerializer
            .Deserialize(messageContent, SharedJsonContext.Default.SignedDebianPackage)
            ?? throw new InvalidOperationException(
                $"JSON string could not be deserialized to '{nameof(SignedDebianPackage)}'." +
                Environment.NewLine +
                $"JSON string was: '{messageContent}'");

        var scriptHandler = new ScriptHandler();
        var signatureValidator = new MinisignSignedDebianPackageSignatureValidator();
        var service = new SystemService(logger, scriptHandler, signatureValidator);

        var result = await service
            .InstallSignedDebianPackage(
                signedDebianPackage,
                EnvironmentVariables.PublicKeys.Value,
                cancellationToken)
            .ConfigureAwait(false);

        return MessageSerializer.Serialize(result);
    }
}
