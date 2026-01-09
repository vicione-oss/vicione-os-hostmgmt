using System.Net;
using System.Net.Sockets;

namespace HostManagement.Shared.Validation.Utils;

/// <summary>
/// Provides utility methods to determine if an IPv4 or IPv6 address is link-local, multicast etc.
/// </summary>
internal static class IPAddressValidatorUtils
{
    private static readonly IPNetwork s_ipv4LoopbackNetwork = IPNetwork.Parse("127.0.0.0/8");
    private static readonly IPNetwork s_ipv6LoopbackNetwork = IPNetwork.Parse("::1/128");

    private static readonly IPNetwork s_ipv4LinkLocalNetwork = IPNetwork.Parse("169.254.0.0/16");
    private static readonly IPNetwork s_ipv6LinkLocalNetwork = IPNetwork.Parse("fe80::/10");

    private static readonly IPNetwork s_ipv4MulticastNetwork = IPNetwork.Parse("224.0.0.0/4");
    private static readonly IPNetwork s_ipv6MulticastNetwork = IPNetwork.Parse("ff00::/8");

    /// <summary>
    /// A string representing the IPv4 loopback network in CIDR notation.
    /// </summary>
    internal static string IPv4LoopbackNetworkCIDR { get; } = s_ipv4LoopbackNetwork.ToString();

    /// <summary>
    /// A string representing the IPv6 loopback network in CIDR notation.
    /// </summary>
    internal static string IPv6LoopbackNetworkCIDR { get; } = s_ipv6LoopbackNetwork.ToString();

    /// <summary>
    /// A string representing the IPv4 link-local network in CIDR notation.
    /// </summary>
    internal static string IPv4LinkLocalNetworkCIDR { get; } = s_ipv4LinkLocalNetwork.ToString();

    /// <summary>
    /// A string representing the IPv6 link-local network in CIDR notation.
    /// </summary>
    internal static string IPv6LinkLocalNetworkCIDR { get; } = s_ipv6LinkLocalNetwork.ToString();

    /// <summary>
    /// A string representing the IPv4 multicast network in CIDR notation.
    /// </summary>
    internal static string IPv4MulticastNetworkCIDR { get; } = s_ipv4MulticastNetwork.ToString();

    /// <summary>
    /// A string representing the IPv6 multicast network in CIDR notation.
    /// </summary>
    internal static string IPv6MulticastNetworkCIDR { get; } = s_ipv6MulticastNetwork.ToString();

    /// <summary>
    /// Determines if an IPv4 address is a loopback address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <returns><see langword="true"/> if loopback otherwise <see langword="false"/></returns>
    internal static bool IsIPv4Loopback(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
            return false;

        if (s_ipv4LoopbackNetwork.Contains(address))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if an IPv6 address is a loopback address.
    /// </summary>
    /// <param name="address">The IPv6 address.</param>
    /// <returns><see langword="true"/> if loopback otherwise <see langword="false"/></returns>
    internal static bool IsIPv6Loopback(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetworkV6)
            return false;

        if (s_ipv6LoopbackNetwork.Contains(address))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if an IPv4 address is a link-local address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <returns><see langword="true"/> if link-local otherwise <see langword="false"/></returns>
    internal static bool IsIPv4LinkLocal(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
            return false;

        if (s_ipv4LinkLocalNetwork.Contains(address))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if an IPv6 address is a link-local address.
    /// </summary>
    /// <param name="address">The IPv6 address.</param>
    /// <returns><see langword="true"/> if link-local otherwise <see langword="false"/></returns>
    internal static bool IsIPv6LinkLocal(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetworkV6)
            return false;

        if (s_ipv6LinkLocalNetwork.Contains(address))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if an IPv4 address is a multicast address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <returns><see langword="true"/> if multicast otherwise <see langword="false"/></returns>
    internal static bool IsIPv4Multicast(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
            return false;

        if (s_ipv4MulticastNetwork.Contains(address))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if an IPv6 address is a multicast address.
    /// </summary>
    /// <param name="address">The IPv6 address.</param>
    /// <returns><see langword="true"/> if multicast otherwise <see langword="false"/></returns>
    internal static bool IsIPv6Multicast(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetworkV6)
            return false;

        if (s_ipv6MulticastNetwork.Contains(address))
            return true;

        return false;
    }
}

