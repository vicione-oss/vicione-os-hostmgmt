using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkNTPSettings
{
    public bool? Enabled { get; set; }
    public List<string>? Servers { get; set; } = [];

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.AddRange(Servers);

        return hashCode.ToHashCode();
    }

    public bool Equals(NetworkNTPSettings? other)
        => other is not null
        && EqualityComparer<bool?>.Default.Equals(Enabled, other.Enabled)
        && EqualityComparer<string>.Default.SequenceEqual(Servers, other.Servers);
}
