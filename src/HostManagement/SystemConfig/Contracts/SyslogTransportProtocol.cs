using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<SyslogTransportProtocol>))]
internal enum SyslogTransportProtocol
{
    [JsonStringEnumMemberName("udp")]
    Udp,

    [JsonStringEnumMemberName("tcp")]
    Tcp
}
