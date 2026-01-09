using System.Net;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Tests.Validation;

internal static partial class ValidationTestData
{
    internal static class Domain
    {
        public const string InvalidDomain = "-local";

        public const string Simple = "local";
        public const string FQDN = "myCompany.com";
    }

    internal static class Hostname
    {
        public const string InvalidHostname = Domain.InvalidDomain;

        public const string Simple = "hostname";
        public const string FQDN = "hostname.device42";
    }

    internal static class StaticHost
    {
        public static readonly StaticHostDetail InvalidLoopback = new()
        {
            Hostname = "Loopback",
            IPAddress = IPAddress.Parse("127.0.0.1")
        };

        public static readonly StaticHostDetail InvalidLinkLocal = new()
        {
            Hostname = "LinkLocal",
            IPAddress = IPAddress.Parse("169.254.0.1")
        };

        public static readonly StaticHostDetail InvalidMulticast = new()
        {
            Hostname = "Multicast",
            IPAddress = IPAddress.Parse("224.0.0.1")
        };

        public static readonly StaticHostDetail InvalidBroadcast = new()
        {
            Hostname = "Broadcast",
            IPAddress = IPAddress.Parse("255.255.255.255")
        };

        public static readonly StaticHostDetail StaticHost1 = new()
        {
            Hostname = "Device1",
            IPAddress = IPAddress.Parse("172.16.0.11")
        };

        public static readonly StaticHostDetail StaticHost2 = new()
        {
            Hostname = "Device2",
            IPAddress = IPAddress.Parse("172.16.0.12")
        };
    }

    internal static class NameServer
    {
        public const string InvalidLinkLocalIPv4 = "169.254.0.1";
        public const string InvalidLinkLocalIPv6 = "fe80::1";
        public const string InvalidMulticastIPv4 = "224.0.0.1";
        public const string InvalidMulticastIPv6 = "ff00::1";
        public const string InvalidBroadcast = "255.255.255.255";

        public const string IPv4 = "1.1.1.1";
        public const string IPv6 = "2606:4700:4700::1111";
    }

    internal static class PrimarySuffix
    {
        public const string InvalidSuffix = Domain.InvalidDomain;

        public const string Simple = Domain.Simple;
        public const string FQDN = Domain.FQDN;
    }

    internal static class SearchDomain
    {
        public const string InvalidDomain = Domain.InvalidDomain;

        public const string Simple = Domain.Simple;
        public const string FQDN = Domain.FQDN;
    }
}
