using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Tests.Validation;

internal static partial class ValidationTestData
{
    internal static class Proxy
    {
        public static readonly NetworkProxyDetail HTTP = new()
        {
            Enabled = true,
            Server = "http://test.de",
            Port = 8080,
            Username = null,
            Password = null
        };

        public static readonly NetworkProxyDetail HTTPS = new()
        {
            Enabled = true,
            Server = "https://test.de",
            Port = 443,
            Username = "user",
            Password = "password"
        };
    }

    internal static class NoProxyItem
    {
        public const string InvalidNoProxyItem = "192.168.444.333";

        public const string IPv4 = "192.168.178.42";
        public const string IPv4Port = "192.168.178.42:443";
        public const string IPv6 = "2606:4700:4700::1111";
        public const string IPv6Port = "[2606:4700:4700::1111]:443";
        public const string IPv6LinkLocal = $"fe80::1%{InterfaceName.Static}";
        public const string IPv6LinkLocalPort = $"[fe80::1%{InterfaceName.Static}]:443";
        public const string Simple = "mySimpleDomainname";
        public const string FQDN = "my.fully-qualified-domain.com";

    }
}
