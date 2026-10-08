using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<SyslogLogFormat>))]
internal enum SyslogLogFormat
{
    [JsonStringEnumMemberName("rfc3164")]
    Rfc3164,

    [JsonStringEnumMemberName("rfc5424")]
    Rfc5424
}
