using System.Text.Json.Serialization;

namespace HostManagement.Shared.Tests.Serialization;

/// <summary>
/// A contract that is deliberately not part of any registered <see cref="JsonSerializerContext"/>.
/// </summary>
internal record UnregisteredContract
{
    public required string Name { get; init; }
}

/// <summary>
/// A contract that is only resolvable after <see cref="AdditionalContractContext"/> has been registered.
/// </summary>
internal record AdditionalContract
{
    public required string Name { get; init; }
}

[JsonSerializable(typeof(AdditionalContract))]
internal partial class AdditionalContractContext : JsonSerializerContext;
