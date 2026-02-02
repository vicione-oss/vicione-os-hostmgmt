using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RenewDHCPLeaseRequestHandler"/>.
/// </summary>
internal sealed class RenewDHCPLeaseRequestHandler(ILogger logger) : RequestHandler<RenewDHCPLeaseResult>
{
    /// <summary>
    /// The topic of the <see cref="RenewDHCPLeaseRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.RenewDHCPLease;

    public override JsonTypeInfo<RenewDHCPLeaseResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.RenewDHCPLeaseResult;

    /// <summary>
    /// Handles the renew DHCP lease request.
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
            var result = await service.RenewDHCPLease(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(RenewDHCPLeaseResult)} could not be serialized into a string.");
        }
    }
}
