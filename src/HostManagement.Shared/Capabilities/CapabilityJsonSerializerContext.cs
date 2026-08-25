using System.Text.Json.Serialization;

namespace HostManagement.Shared.Capabilities;

/// <summary>
/// Provides source-generated JSON metadata for capability related types.
/// </summary>
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(CapabilityStatus))]
public sealed partial class CapabilityJsonSerializerContext : JsonSerializerContext;
