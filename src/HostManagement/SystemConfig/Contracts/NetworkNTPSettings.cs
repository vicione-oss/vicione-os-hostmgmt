namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkNTPSettings
{
    public bool? NTPServersEnabled { get; set; }
    public List<string>? NTPServers { get; set; } = [];

    public override int GetHashCode()
    {
        HashCode hashCode = new();

        hashCode.Add(NTPServersEnabled);
        hashCode.AddRange(NTPServers, EqualityComparer<string>.Default);

        return hashCode.ToHashCode();
    }

    public bool Equals(NetworkNTPSettings? other) =>
        other is not null &&
        EqualityComparer<bool?>.Default.Equals(NTPServersEnabled, other.NTPServersEnabled) &&
        EqualityComparer<string>.Default.SequenceEqual(NTPServers, other.NTPServers);
}
