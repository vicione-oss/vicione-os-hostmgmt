using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record NoProxySettings
{
    public bool? Enabled { get; set; }
    public List<string>? Entries { get; set; }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.AddRange(Entries);

        return hashCode.ToHashCode();
    }

    public bool Equals(NoProxySettings? other)
        => other is not null
        && EqualityComparer<bool?>.Default.Equals(Enabled, other.Enabled)
        && EqualityComparer<string>.Default.SequenceEqual(Entries, other.Entries);
}
