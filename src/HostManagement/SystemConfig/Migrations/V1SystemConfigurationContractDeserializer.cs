using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using V1Contracts = HostManagement.SystemConfig.Migrations.Contracts.V1;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Deserializes version 1 system configuration contracts.
/// </summary>
internal sealed class V1SystemConfigurationContractDeserializer
    : SystemConfigurationContractDeserializer<V1Contracts.SystemConfiguration>
{
    private static readonly JsonSerializerOptions s_serializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter<V1Contracts.ServiceState>(),
            new V1Contracts.DateTimeOffsetConverter(),
            new V1Contracts.IPAddressConverter()
        },
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true
    };

    private static readonly V1Contracts.V1SystemConfigurationJsonSerializerContext s_context =
        new(s_serializerOptions);

    /// <inheritdoc/>
    public override int Version => 1;

    /// <inheritdoc/>
    protected override JsonTypeInfo<V1Contracts.SystemConfiguration> ConfigurationTypeInfo
        => s_context.SystemConfiguration;
}
