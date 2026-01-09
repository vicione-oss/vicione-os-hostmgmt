using System.Text.Json.Serialization;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the <see cref="SourceGenerationContext"/>.
/// </summary>
[JsonSourceGenerationOptions(
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(NamedPipeMessage))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(LoadSystemConfigurationResult))]
[JsonSerializable(typeof(ApplySystemConfigurationResult))]
[JsonSerializable(typeof(GetViciOneSuiteVersionsResult))]
[JsonSerializable(typeof(InstallViciOneSuiteVersionResult))]
[JsonSerializable(typeof(RedeployScriptsResult))]
[JsonSerializable(typeof(GetNetworkStatusInformationResult))]
public partial class SourceGenerationContext : JsonSerializerContext;
