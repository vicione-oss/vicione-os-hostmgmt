using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true,
    Converters = [typeof(DateTimeOffsetConverter), typeof(IPAddressConverter),])]
[JsonSerializable(typeof(SystemConfiguration))]
[JsonSerializable(typeof(DHCPLease))]
internal partial class SystemConfigurationJsonSerializerContext : JsonSerializerContext;
