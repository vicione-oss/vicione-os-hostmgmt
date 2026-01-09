using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.ViciOneSuite;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetViciOneSuiteVersionsRequestHandler"/>.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class GetViciOneSuiteVersionsRequestHandler(ILogger logger)
    : RequestHandler<GetViciOneSuiteVersionsResult>
{
    /// <summary>
    /// The topic of the <see cref="GetViciOneSuiteVersionsRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetViciOneSuiteVersions;

    public override JsonTypeInfo<GetViciOneSuiteVersionsResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;

    /// <summary>
    /// Handles the get suite versions request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var manager = new ViciOneSuiteManager(logger);
            var result = await manager.GetNonPreReleaseSuiteVersions(cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetViciOneSuiteVersionsResult)} could not be serialized into a string.");
        }
    }
}
