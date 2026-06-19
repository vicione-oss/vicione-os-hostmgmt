using System.Text.Json.Serialization;

// Keep namespace of existing 'HostManagement.Shared.Communication' JSON source generation context.
namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the <see cref="CapabilitySourceGenerationContext"/>.
/// </summary>
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(GetSupportedCapabilitiesResult))]
public partial class CapabilitySourceGenerationContext : JsonSerializerContext;
