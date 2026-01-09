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
[JsonSerializable(typeof(GetViciOneSuiteVersionsResult))]
[JsonSerializable(typeof(InstallViciOneSuiteVersionResult))]
[JsonSerializable(typeof(RedeployScriptsResult))]
[JsonSerializable(typeof(GetNetworkStatusInformationResult))]
[JsonSerializable(typeof(RenewDHCPLeaseResult))]
[JsonSerializable(typeof(ServiceControlResult))]
[JsonSerializable(typeof(ResetSystemResult))]
[JsonSerializable(typeof(RestartSystemResult))]
public partial class SourceGenerationContext : JsonSerializerContext;
