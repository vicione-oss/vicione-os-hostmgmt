using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkNTPSettings"/>.
/// </summary>
public sealed record NetworkNTPSettings
{
    /// <summary>
    /// Represents the empty NTP settings.
    /// </summary>
    [JsonIgnore]
    public static NetworkNTPSettings Empty { get; } = new NetworkNTPSettings();

    /// <summary>
    /// The list of fallback NTP servers (IPv4, IPv6 addresses and DNS host names).
    /// </summary>
    [JsonRequired]
    public List<string> FallbackNTPServers { get; init; } = [];

    /// <summary>
    /// The list of NTP servers (IPv4, IPv6 addresses and DNS host names).
    /// </summary>
    [JsonRequired]
    public List<string> NTPServers { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(FallbackNTPServers.GetSequenceHashCode());
        hashCode.Add(NTPServers.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(NetworkNTPSettings? other) =>
        other is not null &&
        FallbackNTPServers.SequenceEqual(other.FallbackNTPServers) &&
        NTPServers.SequenceEqual(other.NTPServers);
}
