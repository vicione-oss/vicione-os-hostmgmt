using System.Text.Json;
using HostManagement.NetworkStatus;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetNetworkStatusInformationRequestHandler"/>.
/// </summary>
internal sealed class GetNetworkStatusInformationRequestHandler : IRequestHandler
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="GetNetworkStatusInformationRequestHandler"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public GetNetworkStatusInformationRequestHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// The topic of the <see cref="GetViciOneSuiteVersionsRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.GetNetworkStatusInformation;

    /// <summary>
    /// Handles the get network status information request.
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

            var manager = new NetworkStatusInformationManager(_logger);
            var result = await manager.GetNetworkStatusInformation(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.GetNetworkStatusInformationResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetNetworkStatusInformationResult)} could not be serialized into a string.");
        }
    }
}
