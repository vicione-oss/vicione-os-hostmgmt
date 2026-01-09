using System.Text.Json;
using Microsoft.Extensions.Logging;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts.System;
using HostManagement.Services;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;
using SharedJsonContext = HostManagement.Shared.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="InstallSignedDebianPackageRequestHandler"/>.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class InstallSignedDebianPackageRequestHandler(ILogger logger) : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="InstallSignedDebianPackageRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.InstallSignedDebianPackage;

    /// <summary>
    /// Handles the install signed Debian package request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var signedDebianPackage = JsonSerializer
                .Deserialize(messageContent, SharedJsonContext.Default.SignedDebianPackage)
                ?? throw new InvalidOperationException(
                    $"JSON string could not be deserialized to '{nameof(SignedDebianPackage)}'." +
                    Environment.NewLine +
                    $"JSON string was: '{messageContent}'");

            var scriptResult = new Result();
            var scriptHandler = new ScriptHandler(scriptResult);
            var service = new SystemService(logger, scriptHandler, scriptResult);
            var result = await service.InstallSignedDebianPackage(
                signedDebianPackage,
                EnvironmentVariables.PublicKeys,
                cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, CommunicationJsonContext.Default.SystemControlResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(SystemControlResult)} could not be serialized into a string.");
        }
    }
}
