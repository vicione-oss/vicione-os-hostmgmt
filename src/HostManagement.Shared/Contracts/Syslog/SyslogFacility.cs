using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.Syslog;

/// <summary>
/// Represents standard syslog facility identifiers used to categorize log messages.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SyslogFacility>))]
public enum SyslogFacility
{
    /// <summary>
    /// Kernel messages.
    /// </summary>
    [JsonStringEnumMemberName("kern")]
    Kern,

    /// <summary>
    /// User-level messages.
    /// </summary>
    [JsonStringEnumMemberName("user")]
    User,

    /// <summary>
    /// Mail system messages.
    /// </summary>
    [JsonStringEnumMemberName("mail")]
    Mail,

    /// <summary>
    /// System daemon messages.
    /// </summary>
    [JsonStringEnumMemberName("daemon")]
    Daemon,

    /// <summary>
    /// Authentication and authorization messages.
    /// </summary>
    [JsonStringEnumMemberName("auth")]
    Auth,

    /// <summary>
    /// Messages generated internally by the syslog system.
    /// </summary>
    [JsonStringEnumMemberName("syslog")]
    Syslog,

    /// <summary>
    /// Line printer subsystem messages.
    /// </summary>
    [JsonStringEnumMemberName("lpr")]
    Lpr,

    /// <summary>
    /// Network news subsystem messages.
    /// </summary>
    [JsonStringEnumMemberName("news")]
    News,

    /// <summary>
    /// UUCP subsystem messages.
    /// </summary>
    [JsonStringEnumMemberName("uucp")]
    Uucp,

    /// <summary>
    /// Cron scheduler messages.
    /// </summary>
    [JsonStringEnumMemberName("cron")]
    Cron,

    /// <summary>
    /// Private authentication and authorization messages.
    /// </summary>
    [JsonStringEnumMemberName("authpriv")]
    Authpriv,

    /// <summary>
    /// FTP daemon messages.
    /// </summary>
    [JsonStringEnumMemberName("ftp")]
    Ftp,

    /// <summary>
    /// Network Time Protocol (NTP) messages.
    /// </summary>
    [JsonStringEnumMemberName("ntp")]
    Ntp,

    /// <summary>
    /// Security-related messages.
    /// </summary>
    [JsonStringEnumMemberName("security")]
    Security,

    /// <summary>
    /// Console messages.
    /// </summary>
    [JsonStringEnumMemberName("console")]
    Console,

    /// <summary>
    /// Solaris-specific cron scheduler messages.
    /// </summary>
    [JsonStringEnumMemberName("solaris-cron")]
    SolarisCron,

    /// <summary>
    /// Local use facility 0.
    /// </summary>
    [JsonStringEnumMemberName("local0")]
    Local0,

    /// <summary>
    /// Local use facility 1.
    /// </summary>
    [JsonStringEnumMemberName("local1")]
    Local1,

    /// <summary>
    /// Local use facility 2.
    /// </summary>
    [JsonStringEnumMemberName("local2")]
    Local2,

    /// <summary>
    /// Local use facility 3.
    /// </summary>
    [JsonStringEnumMemberName("local3")]
    Local3,

    /// <summary>
    /// Local use facility 4.
    /// </summary>
    [JsonStringEnumMemberName("local4")]
    Local4,

    /// <summary>
    /// Local use facility 5.
    /// </summary>
    [JsonStringEnumMemberName("local5")]
    Local5,

    /// <summary>
    /// Local use facility 6.
    /// </summary>
    [JsonStringEnumMemberName("local6")]
    Local6,

    /// <summary>
    /// Local use facility 7.
    /// </summary>
    [JsonStringEnumMemberName("local7")]
    Local7
}
