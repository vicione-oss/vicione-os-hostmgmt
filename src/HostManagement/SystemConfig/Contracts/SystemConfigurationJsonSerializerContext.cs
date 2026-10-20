using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true,
    Converters = [typeof(DateTimeOffsetConverter), typeof(IPAddressConverter), typeof(PhysicalAddressConverter)])]
[JsonSerializable(typeof(SystemConfiguration))]
[JsonSerializable(typeof(DHCPLease))]
[JsonSerializable(typeof(NTPSyncStatus))]
internal partial class SystemConfigurationJsonSerializerContext : JsonSerializerContext;
