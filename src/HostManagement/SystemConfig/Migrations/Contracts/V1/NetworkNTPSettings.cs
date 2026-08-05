using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

internal sealed record NetworkNTPSettings
{
    public List<string>? FallbackNTPServers { get; set; }
    public bool? NTPServersEnabled { get; set; }
    public List<string>? NTPServers { get; set; } = [];

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.AddRange(FallbackNTPServers, EqualityComparer<string>.Default);
        hashCode.Add(NTPServersEnabled);
        hashCode.AddRange(NTPServers, EqualityComparer<string>.Default);
        return hashCode.ToHashCode();
    }

    public bool Equals(NetworkNTPSettings? other) =>
        other is not null &&
        EqualityComparer<string>.Default.SequenceEqual(FallbackNTPServers, other.FallbackNTPServers) &&
        EqualityComparer<bool?>.Default.Equals(NTPServersEnabled, other.NTPServersEnabled) &&
        EqualityComparer<string>.Default.SequenceEqual(NTPServers, other.NTPServers);
}
