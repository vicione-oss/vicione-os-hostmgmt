using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.Syslog;

/// <summary>
/// Represents standard syslog severity levels used to indicate the importance and urgency of log messages.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SyslogLevel>))]
public enum SyslogLevel
{
    /// <summary>
    /// System is unusable.
    /// </summary>
    [JsonStringEnumMemberName("emerg")]
    Emerg,

    /// <summary>
    /// Immediate action is required.
    /// </summary>
    [JsonStringEnumMemberName("alert")]
    Alert,

    /// <summary>
    /// Critical conditions.
    /// </summary>
    [JsonStringEnumMemberName("crit")]
    Crit,

    /// <summary>
    /// Error conditions.
    /// </summary>
    [JsonStringEnumMemberName("err")]
    Err,

    /// <summary>
    /// Warning conditions.
    /// </summary>
    [JsonStringEnumMemberName("warning")]
    Warning,

    /// <summary>
    /// Normal but significant conditions.
    /// </summary>
    [JsonStringEnumMemberName("notice")]
    Notice,

    /// <summary>
    /// Informational messages.
    /// </summary>
    [JsonStringEnumMemberName("info")]
    Info,

    /// <summary>
    /// Debug-level messages intended for diagnostics and troubleshooting.
    /// </summary>
    [JsonStringEnumMemberName("debug")]
    Debug
}
