using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<SyslogLevel>))]
internal enum SyslogLevel
{
    [JsonStringEnumMemberName("emerg")]
    Emerg,

    [JsonStringEnumMemberName("alert")]
    Alert,

    [JsonStringEnumMemberName("crit")]
    Crit,

    [JsonStringEnumMemberName("err")]
    Err,

    [JsonStringEnumMemberName("warning")]
    Warning,

    [JsonStringEnumMemberName("notice")]
    Notice,

    [JsonStringEnumMemberName("info")]
    Info,

    [JsonStringEnumMemberName("debug")]
    Debug
}
