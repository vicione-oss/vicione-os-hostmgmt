using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
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

    public override JsonTypeInfo<SystemControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.SystemControlResult;

    /// <summary>
    /// Handles the install signed Debian package request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
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

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
