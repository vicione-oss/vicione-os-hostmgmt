using System.Text.Json.Serialization;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Contracts.System;

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
[JsonSerializable(typeof(DHCPLease))]
[JsonSerializable(typeof(NetworkStatusInformation))]
[JsonSerializable(typeof(ServiceControlRequest))]
[JsonSerializable(typeof(SignedDebianPackage))]
public partial class SourceGenerationContext : JsonSerializerContext;
