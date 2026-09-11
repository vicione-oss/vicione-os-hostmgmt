using System.Net;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Tests.Validation;

internal static partial class ValidationTestData
{
    internal static class Gateway
    {
        public static readonly IPAddress InvalidNotRoutableIPv4 = IPAddress.Parse("0.0.0.0");
        public static readonly IPAddress InvalidLoopbackIPv4 = IPAddress.Parse("127.0.0.1");
        public static readonly IPAddress InvalidMulticastIPv4 = IPAddress.Parse("224.0.0.1");
        public static readonly IPAddress InvalidBroadcastIPv4 = IPAddress.Parse("255.255.255.255");

        public static readonly IPAddress Gateway1 = IPAddress.Parse("192.168.123.1");
        public static readonly IPAddress Gateway2 = IPAddress.Parse("192.168.123.2");
    }

    internal static class InterfaceName
    {
        public const string Static = "static";
        public const string DHCP = "dhcp";
    }

    internal static class Interface
    {
        public static readonly NetworkInterfaceDetail Static = new()
        {
            CommonInformation = new()
            {
                Enabled = true,
                Name = InterfaceName.Static,
                UserDefinedMACAddress = new()
                {
                    Enabled = true,
                    Address = "02-23-45-67-89-AB"
                }
            },
            IPv4 = new()
            {
                DHCPEnabled = false,
                Gateway = Gateway.Gateway1,
                IPv4Details =
                [
                    new()
                    {
                        IPAddress = IPAddress.Parse("192.168.123.45"),
                        Netmask = IPAddress.Parse("255.255.255.0")
                    }
                ]
            }
        };

        public static readonly NetworkInterfaceDetail DHCP = new()
        {
            CommonInformation = new()
            {
                Enabled = true,
                Name = InterfaceName.DHCP,
                UserDefinedMACAddress = new()
                {
                    Enabled = true,
                    Address = "02-23-45-67-89-AC"
                }
            },
            IPv4 = new()
            {
                DHCPEnabled = true
            }
        };
    }

    internal static class Netmask
    {
        public static readonly IPAddress InvalidIPv6 = IPAddress.Parse("ffff:0000:ffff:ffff::");

        public static readonly IPAddress ValidIPv4 = IPAddress.Parse("255.255.255.0");
    }
}
