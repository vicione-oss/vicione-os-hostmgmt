using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Syslog;

/// <summary>
/// Represents the syslog settings.
/// </summary>
public sealed record SyslogSettings
{
    /// <summary>
    /// Determines whether syslog settings are enabled. [default: false]
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Destination (unicast IP:PORT or multicast GROUP:PORT).
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The transport protocol. [default: UDP]
    /// </summary>
    public SyslogTransportProtocol TransportProtocol { get; set; } = SyslogTransportProtocol.Udp;

    /// <summary>
    /// The log format. [default: RFC5424, legacy: RFC3164]
    /// </summary>
    public SyslogLogFormat LogFormat { get; set; } = SyslogLogFormat.Rfc5424;

    /// <summary>
    /// The reconnect delay in seconds. [default: 30, min: 1]
    /// </summary>
    public int ConnectionRetrySeconds { get; set; } = 30;

    /// <summary>
    /// The facilities to exclude (e.g.: auth, authpriv). [default: NONE]
    /// </summary>
    public List<SyslogFacility> ExcludeSyslogFacilities { get; init; } = [];

    /// <summary>
    /// The log levels to exclude (e.g.: debug, info). [default: NONE]
    /// </summary>
    public List<SyslogLevel> ExcludeSyslogLevels { get; init; } = [];

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool Equals(SyslogSettings? other)
        => other is not null
        && Enabled.Equals(other.Enabled)
        && Address.Equals(other.Address)
        && TransportProtocol.Equals(other.TransportProtocol)
        && LogFormat.Equals(other.LogFormat)
        && ConnectionRetrySeconds.Equals(other.ConnectionRetrySeconds)
        && ExcludeSyslogFacilities.OrderBy(facility => facility)
            .SequenceEqual(other.ExcludeSyslogFacilities.OrderBy(facility => facility))
        && ExcludeSyslogLevels.OrderBy(level => level)
            .SequenceEqual(other.ExcludeSyslogLevels.OrderBy(level => level));
}
