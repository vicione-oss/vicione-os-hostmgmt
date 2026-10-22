using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the <see cref="SourceGenerationContext"/>.
/// </summary>
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(MessageHeader))]
[JsonSerializable(typeof(NamedPipeMessage))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(GetSupportedCapabilitiesResult))]
[JsonSerializable(typeof(GetSystemConfigurationResult))]
[JsonSerializable(typeof(SetSystemConfigurationResult))]
[JsonSerializable(typeof(GetOriginalPhysicalAddressResult))]
[JsonSerializable(typeof(GetNetworkStatusInformationResult))]
[JsonSerializable(typeof(GetDHCPLeaseInformationResult))]
[JsonSerializable(typeof(GetNTPFallbackInformationResult))]
[JsonSerializable(typeof(GetNTPSyncStatusResult))]
[JsonSerializable(typeof(GetServiceStateResult))]
[JsonSerializable(typeof(RedeployScriptsResult))]
[JsonSerializable(typeof(ServiceControlResult))]
[JsonSerializable(typeof(SystemControlResult))]
// A JSON serializer context aggregates every message contract by design, so its class coupling is exactly the
// number of serializable contracts. The metric carries no signal for this type.
[SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "A JSON serializer context aggregates all message contracts by design.")]
public partial class SourceGenerationContext : JsonSerializerContext;
