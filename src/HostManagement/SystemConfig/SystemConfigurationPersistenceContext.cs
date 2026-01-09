using System.Text.Json.Serialization;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents the <see cref="SourceGenerationContext"/>.
/// </summary>
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    IgnoreReadOnlyFields = true,
    IgnoreReadOnlyProperties = true,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true,
    WriteIndented = true)]
[JsonSerializable(typeof(SystemConfiguration))]
internal sealed partial class SystemConfigurationPersistenceContext : JsonSerializerContext;
