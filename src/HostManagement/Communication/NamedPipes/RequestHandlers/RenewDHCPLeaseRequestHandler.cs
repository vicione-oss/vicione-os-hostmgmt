using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RenewDHCPLeaseRequestHandler"/>.
/// </summary>
internal sealed class RenewDHCPLeaseRequestHandler(ILogger logger) : RequestHandler<GetDHCPLeaseInformationResult>
{
    /// <summary>
    /// The topic of the <see cref="RenewDHCPLeaseRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.RenewDHCPLease;

    public override JsonTypeInfo<GetDHCPLeaseInformationResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetDHCPLeaseInformationResult;

    /// <summary>
    /// Handles the renew DHCP lease request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var service = new DHCPLeaseService(logger);
            var result = await service.RenewDHCPLease(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetDHCPLeaseInformationResult)} could not be serialized into a string.");
        }
    }
}
