using System.Text.Json.Serialization;

namespace HostManagement.Communication;

/// <summary>
/// Represents the <see cref="CliSourceGenerationContext"/>.
/// </summary>
[JsonSourceGenerationOptions(UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(CliMessageContent))]
internal partial class CliSourceGenerationContext : JsonSerializerContext;
