using System.Text.Json;
using Microsoft.Extensions.Logging;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetDHCPLeaseInformationRequestHandler"/>.
/// </summary>
internal sealed class GetDHCPLeaseInformationRequestHandler(ILogger logger) : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="GetDHCPLeaseInformationRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.GetDHCPLeaseInformation;

    /// <summary>
    /// Handles the get DHCP lease information request.
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

            var service = new DHCPLeaseService(logger);
            var result = await service.GetDHCPLeaseInformation(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.GetDHCPLeaseInformationResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetDHCPLeaseInformationResult)} could not be serialized into a string.");
        }
    }
}
