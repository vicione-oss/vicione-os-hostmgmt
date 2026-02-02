using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetDHCPLeaseInformationRequestHandler"/>.
/// </summary>
internal sealed class GetDHCPLeaseInformationRequestHandler(ILogger logger)
    : RequestHandler<GetDHCPLeaseInformationResult>
{
    /// <summary>
    /// The topic of the <see cref="GetDHCPLeaseInformationRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetDHCPLeaseInformation;

    public override JsonTypeInfo<GetDHCPLeaseInformationResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetDHCPLeaseInformationResult;

    /// <summary>
    /// Handles the get DHCP lease information request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var service = new DHCPLeaseService(logger);
            var result = await service.GetDHCPLeaseInformation(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetDHCPLeaseInformationResult)} could not be serialized into a string.");
        }
    }
}
