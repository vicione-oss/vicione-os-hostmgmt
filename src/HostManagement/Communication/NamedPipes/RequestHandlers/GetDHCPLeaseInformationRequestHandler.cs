using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

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
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        var service = new DHCPLeaseService(logger, new ScriptHandler());
        var result = await service.GetDHCPLeaseInformation(messageContent, cancellationToken).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
