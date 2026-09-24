using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.Shared.Tests.Validation;

internal static partial class ValidationTestData
{
    internal static class Syslog
    {
        public const string IPv4Address = "192.168.42.10:514";
        public const string IPv6Address = "[2001:db8::42]:514";
        public const string FQDNAddress = "logs.example.com:514";
        public const string SimpleHostnameAddress = "syslog-server:514";
        public const string MulticastIPv4Address = "239.0.0.1:6000";
        public const string InvalidIPv4Address = "192.168.999.10:514";

        public static SyslogSettings ConfiguredDisabled => new()
        {
            Enabled = false,
            Address = FQDNAddress,
            TransportProtocol = SyslogTransportProtocol.Tcp,
            LogFormat = SyslogLogFormat.Rfc5424,
            ConnectionRetrySeconds = 30,
            ExcludeSyslogFacilities = [SyslogFacility.Auth, SyslogFacility.Authpriv],
            ExcludeSyslogLevels = [SyslogLevel.Debug, SyslogLevel.Info]
        };

        public static SyslogSettings ConfiguredEnabled => ConfiguredDisabled with { Enabled = true };
    }
}
