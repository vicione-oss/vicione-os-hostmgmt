namespace HostManagement.Shared.Tests.Validation;

internal static partial class ValidationTestData
{
    internal static class NTPServer
    {
        public const string InvalidIPv4 = "192.168.444.333";
        public const string InvalidIPv6 = "xe80::";
        public const string InvalidLinkLocalIPv4 = "169.254.0.1";
        public const string InvalidLinkLocalIPv6 = "fe80::1";
        public const string InvalidMulticastIPv4 = "224.0.0.1";
        public const string InvalidMulticastIPv6 = "ff00::1";
        public const string InvalidBroadcast = "255.255.255.255";

        // All NTP servers are 'time.google.com' as of July 2025
        public const string IPv4 = "216.239.35.0";
        public const string IPv6 = "2001:4860:4806::";
        public const string FQDN = "time.google.com";
    }
}
