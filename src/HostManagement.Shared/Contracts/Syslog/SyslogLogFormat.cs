using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.Syslog;

/// <summary>
/// Represents the Syslog message format.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SyslogLogFormat>))]
public enum SyslogLogFormat
{
    /// <summary>
    /// RFC 3164 format (legacy).
    /// </summary>
    [JsonStringEnumMemberName("rfc3164")]
    Rfc3164,

    /// <summary>
    /// RFC 5424 format with newline separator (default).
    /// </summary>
    [JsonStringEnumMemberName("rfc5424")]
    Rfc5424
}
