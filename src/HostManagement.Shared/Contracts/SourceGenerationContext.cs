using System.Text.Json.Serialization;
using HostManagement.Shared.Contracts.NetworkStatus;

namespace HostManagement.Shared.Contracts;

/// <summary>
/// Represents the <see cref="SourceGenerationContext"/>.
/// </summary>
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(SystemConfiguration))]
[JsonSerializable(typeof(NetworkStatusInformation))]
public partial class SourceGenerationContext : JsonSerializerContext;
