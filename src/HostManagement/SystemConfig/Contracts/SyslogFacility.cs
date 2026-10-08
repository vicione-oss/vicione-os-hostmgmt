using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<SyslogFacility>))]
internal enum SyslogFacility
{
    [JsonStringEnumMemberName("kern")]
    Kern,

    [JsonStringEnumMemberName("user")]
    User,

    [JsonStringEnumMemberName("mail")]
    Mail,

    [JsonStringEnumMemberName("daemon")]
    Daemon,

    [JsonStringEnumMemberName("auth")]
    Auth,

    [JsonStringEnumMemberName("syslog")]
    Syslog,

    [JsonStringEnumMemberName("lpr")]
    Lpr,

    [JsonStringEnumMemberName("news")]
    News,

    [JsonStringEnumMemberName("uucp")]
    Uucp,

    [JsonStringEnumMemberName("cron")]
    Cron,

    [JsonStringEnumMemberName("authpriv")]
    Authpriv,

    [JsonStringEnumMemberName("ftp")]
    Ftp,

    [JsonStringEnumMemberName("ntp")]
    Ntp,

    [JsonStringEnumMemberName("security")]
    Security,

    [JsonStringEnumMemberName("console")]
    Console,

    [JsonStringEnumMemberName("solaris-cron")]
    SolarisCron,

    [JsonStringEnumMemberName("local0")]
    Local0,

    [JsonStringEnumMemberName("local1")]
    Local1,

    [JsonStringEnumMemberName("local2")]
    Local2,

    [JsonStringEnumMemberName("local3")]
    Local3,

    [JsonStringEnumMemberName("local4")]
    Local4,

    [JsonStringEnumMemberName("local5")]
    Local5,

    [JsonStringEnumMemberName("local6")]
    Local6,

    [JsonStringEnumMemberName("local7")]
    Local7
}
