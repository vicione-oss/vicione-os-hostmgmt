using System.Text.Json.Serialization;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the <see cref="SourceGenerationContext"/>.
/// </summary>
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(NamedPipeMessage))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(GetSystemConfigurationResult))]
[JsonSerializable(typeof(SetSystemConfigurationResult))]
[JsonSerializable(typeof(GetOriginalPhysicalAddressResult))]
[JsonSerializable(typeof(GetNetworkStatusInformationResult))]
[JsonSerializable(typeof(GetDHCPLeaseInformationResult))]
[JsonSerializable(typeof(GetNTPFallbackInformationResult))]
[JsonSerializable(typeof(GetServiceStateResult))]
[JsonSerializable(typeof(RedeployScriptsResult))]
[JsonSerializable(typeof(ServiceControlResult))]
[JsonSerializable(typeof(SystemControlResult))]
public partial class SourceGenerationContext : JsonSerializerContext;
