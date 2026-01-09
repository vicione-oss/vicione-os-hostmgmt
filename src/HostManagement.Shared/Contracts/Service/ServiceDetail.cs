using System.Text.Json.Serialization;
using HostManagement.Shared.Enums;

namespace HostManagement.Shared.Contracts.Service;

/// <summary>
/// Represents a <see cref="ServiceDetail"/>.
/// </summary>
public sealed record ServiceDetail
{
    /// <summary>
    /// Represents the empty or unknown ServiceDetail.
    /// </summary>
    public static ServiceDetail Empty { get; } =
        new ServiceDetail() { Name = "Empty", State = ServiceState.Unknown };

    /// <summary>
    /// The service name.
    /// </summary>
    [JsonRequired]
    public required string Name { get; set; }

    /// <summary>
    /// The service state.
    /// </summary>
    [JsonRequired]
    public required ServiceState State { get; set; }
}
