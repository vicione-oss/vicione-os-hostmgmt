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
[JsonSerializable(typeof(GetViciOneSuiteVersionsResult))]
[JsonSerializable(typeof(InstallViciOneSuiteVersionResult))]
[JsonSerializable(typeof(RedeployScriptsResult))]
[JsonSerializable(typeof(RenewDHCPLeaseResult))]
[JsonSerializable(typeof(ServiceControlResult))]
[JsonSerializable(typeof(ResetSystemResult))]
[JsonSerializable(typeof(RestartSystemResult))]
[JsonSerializable(typeof(ShutdownSystemResult))]
[JsonSerializable(typeof(UpdateSystemResult))]
public partial class SourceGenerationContext : JsonSerializerContext;
