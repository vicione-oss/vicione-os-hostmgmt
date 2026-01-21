using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="ResetNetworkInterfaceRequestHandler"/>.
/// </summary>
/// <param name="configurationService">The system configuration service.</param>
internal sealed class ResetNetworkInterfaceRequestHandler(SystemConfigurationService configurationService)
    : RequestHandler<SystemControlResult>
{
    /// <summary>
    /// The topic of the <see cref="ResetNetworkInterfaceRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.ResetNetworkInterface;

    public override JsonTypeInfo<SystemControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.SystemControlResult;

    /// <summary>
    /// Handles the reset network interface request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken = default)
    {
        var result = await configurationService
            .ResetNetworkInterface(messageContent, cancellationToken)
            .ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
