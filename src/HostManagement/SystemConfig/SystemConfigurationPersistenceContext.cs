using System.Text.Json.Serialization;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    IgnoreReadOnlyFields = true,
    IgnoreReadOnlyProperties = true,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true,
    WriteIndented = true,
    Converters = [typeof(DateTimeOffsetConverter), typeof(IPAddressConverter),])]
[JsonSerializable(typeof(SystemConfiguration))]
internal sealed partial class SystemConfigurationPersistenceContext : JsonSerializerContext;
