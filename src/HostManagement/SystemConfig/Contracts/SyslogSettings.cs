using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record SyslogSettings
{
    public bool? Enabled { get; set; }
    public string? Address { get; set; }
    public SyslogTransportProtocol? TransportProtocol { get; set; }
    public SyslogLogFormat? LogFormat { get; set; }
    public int? ConnectionRetrySeconds { get; set; }
    public List<SyslogFacility>? ExcludeSyslogFacilities { get; set; }
    public List<SyslogLevel>? ExcludeSyslogLevels { get; set; }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.Add(Address);
        hashCode.Add(TransportProtocol);
        hashCode.Add(LogFormat);
        hashCode.Add(ConnectionRetrySeconds);
        hashCode.AddRange(ExcludeSyslogFacilities, order: facility => facility);
        hashCode.AddRange(ExcludeSyslogLevels, order: level => level);

        return hashCode.ToHashCode();
    }

    public bool Equals(SyslogSettings? other)
        => other is not null
        && EqualityComparer<bool?>.Default.Equals(Enabled, other.Enabled)
        && EqualityComparer<string?>.Default.Equals(Address, other.Address)
        && EqualityComparer<SyslogTransportProtocol?>.Default.Equals(TransportProtocol, other.TransportProtocol)
        && EqualityComparer<SyslogLogFormat?>.Default.Equals(LogFormat, other.LogFormat)
        && EqualityComparer<int?>.Default.Equals(ConnectionRetrySeconds, other.ConnectionRetrySeconds)
        && EqualityComparer<SyslogFacility>.Default.SequenceEqual(ExcludeSyslogFacilities, other.ExcludeSyslogFacilities, order: facility => facility)
        && EqualityComparer<SyslogLevel>.Default.SequenceEqual(ExcludeSyslogLevels, other.ExcludeSyslogLevels, order: level => level);
}
