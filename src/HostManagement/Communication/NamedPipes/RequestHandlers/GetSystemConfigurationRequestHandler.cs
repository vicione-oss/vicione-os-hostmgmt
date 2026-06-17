using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetSystemConfigurationRequestHandler"/>.
/// </summary>
/// <param name="configurationService">The system configuration service.</param>
internal sealed class GetSystemConfigurationRequestHandler(SystemConfigurationService configurationService)
    : RequestHandler<GetSystemConfigurationResult>
{
    /// <summary>
    /// The topic of the <see cref="GetSystemConfigurationRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetSystemConfiguration;

    public override JsonTypeInfo<GetSystemConfigurationResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetSystemConfigurationResult;

    /// <summary>
    /// Handles the get system configuration request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var result = await configurationService.GetConfiguration(cancellationToken).ConfigureAwait(false);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetSystemConfigurationResult)} could not be serialized into a string.");
        }
    }
}
