using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.Syslog;

/// <summary>
/// Represents the syslog transport protocol.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SyslogTransportProtocol>))]
public enum SyslogTransportProtocol
{
    /// <summary>
    /// UDP protocol (default).
    /// </summary>
    [JsonStringEnumMemberName("udp")]
    Udp,

    /// <summary>
    /// TCP protocol.
    /// </summary>
    [JsonStringEnumMemberName("tcp")]
    Tcp
}
