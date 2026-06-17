using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true,
    Converters = [typeof(DateTimeOffsetConverter), typeof(IPAddressConverter),])]
[JsonSerializable(typeof(SystemConfiguration))]
internal sealed partial class V1SystemConfigurationJsonSerializerContext : JsonSerializerContext;
