using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.ViciOneSuite;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetViciOneSuiteVersionsRequestHandler"/>.
/// </summary>
internal sealed class GetViciOneSuiteVersionsRequestHandler : IRequestHandler
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="GetViciOneSuiteVersionsRequestHandler"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public GetViciOneSuiteVersionsRequestHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// The topic of the <see cref="GetViciOneSuiteVersionsRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.GetViciOneSuiteVersions;

    /// <summary>
    /// Handles the get suite versions request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var manager = new ViciOneSuiteManager(_logger);
            var result = await manager.GetNonPreReleaseSuiteVersions(cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.GetViciOneSuiteVersionsResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetViciOneSuiteVersionsResult)} could not be serialized into a string.");
        }
    }
}
