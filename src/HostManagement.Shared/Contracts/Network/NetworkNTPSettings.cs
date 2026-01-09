using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkNTPSettings"/>.
/// </summary>
public sealed record NetworkNTPSettings
{
    /// <summary>
    /// Initializes new <see cref="NetworkNTPSettings"/>.
    /// </summary>
    [JsonConstructor]
    public NetworkNTPSettings(
        List<string>? fallbackNTPServers = null,
        List<string>? ntpServers = null)
    {
        FallbackNTPServers = fallbackNTPServers;
        NTPServers = ntpServers ?? [];
    }

    /// <summary>
    /// Represents the empty NTP settings.
    /// </summary>
    [JsonIgnore]
    public static NetworkNTPSettings Empty { get; } = new NetworkNTPSettings();

    /// <summary>
    /// The list of fallback NTP servers (IPv4, IPv6 addresses and DNS host names).
    /// </summary>
    [Obsolete("This will be removed in a future version. Use 'GetNTPFallbackInformation' pipe request instead.", false)]
    public List<string>? FallbackNTPServers { get; init; }

    /// <summary>
    /// Determines whether NTP servers are enabled.
    /// </summary>
    public bool NTPServersEnabled { get; set; }

    /// <summary>
    /// The list of NTP servers (IPv4, IPv6 addresses and DNS host names).
    /// </summary>
    public List<string> NTPServers { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(FallbackNTPServers?.GetSequenceHashCode());
        hashCode.Add(NTPServersEnabled);
        hashCode.Add(NTPServers.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(NetworkNTPSettings? other) =>
        other is not null &&
        ((FallbackNTPServers is not null &&
          other.FallbackNTPServers is not null &&
          FallbackNTPServers.SequenceEqual(other.FallbackNTPServers)) ||
          FallbackNTPServers == other.FallbackNTPServers) &&
        NTPServersEnabled.Equals(other.NTPServersEnabled) &&
        NTPServers.SequenceEqual(other.NTPServers);
}
