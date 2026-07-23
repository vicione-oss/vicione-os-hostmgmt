namespace HostManagement.SystemConfig.Contracts;

internal sealed record SearchDomainsSettings
{
    public bool? Enabled { get; set; }
    public List<string>? Domains { get; set; }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.AddRange(Domains, EqualityComparer<string>.Default);

        return hashCode.ToHashCode();
    }

    public bool Equals(SearchDomainsSettings? other)
        => other is not null
        && EqualityComparer<bool?>.Default.Equals(Enabled, other.Enabled)
        && EqualityComparer<string>.Default.SequenceEqual(Domains, other.Domains);
}
