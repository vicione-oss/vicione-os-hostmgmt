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

    private static readonly IPNetwork s_ipv4BroadcastNetwork = IPNetwork.Parse("255.255.255.255/32");

    /// <summary>
    /// A string representing the IPv4 loopback network in CIDR notation ("127.0.0.0/8").
    /// </summary>
    internal static string IPv4LoopbackNetworkCIDR { get; } = s_ipv4LoopbackNetwork.ToString();

    /// <summary>
    /// A string representing the IPv6 loopback network in CIDR notation ("::1/128").
    /// </summary>
    internal static string IPv6LoopbackNetworkCIDR { get; } = s_ipv6LoopbackNetwork.ToString();

    /// <summary>
    /// A string representing the IPv4 link-local network in CIDR notation ("169.254.0.0/16").
    /// </summary>
    internal static string IPv4LinkLocalNetworkCIDR { get; } = s_ipv4LinkLocalNetwork.ToString();

    /// <summary>
    /// A string representing the IPv6 link-local network in CIDR notation ("fe80::/10").
    /// </summary>
    internal static string IPv6LinkLocalNetworkCIDR { get; } = s_ipv6LinkLocalNetwork.ToString();

    /// <summary>
    /// A string representing the IPv4 multicast network in CIDR notation ("224.0.0.0/4").
    /// </summary>
    internal static string IPv4MulticastNetworkCIDR { get; } = s_ipv4MulticastNetwork.ToString();

    /// <summary>
    /// A string representing the IPv6 multicast network in CIDR notation ("ff00::/8").
    /// </summary>
    internal static string IPv6MulticastNetworkCIDR { get; } = s_ipv6MulticastNetwork.ToString();

    /// <summary>
    /// A string representing the IPv4 broadcast network in CIDR notation ("255.255.255.255/32").
    /// </summary>
    internal static string IPv4BroadcastNetworkCIDR { get; } = s_ipv4BroadcastNetwork.ToString();

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

    /// <summary>
    /// Determines if an IPv4 address is a broadcast address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <returns><see langword="true"/> if broadcast otherwise <see langword="false"/></returns>
    internal static bool IsIPv4Broadcast(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
            return false;

        if (s_ipv4BroadcastNetwork.Contains(address))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if a string is a potential IPv4 address.
    /// </summary>
    /// <param name="potentialIPv4Address">The potential IPv4 address.</param>
    /// <returns><see langword="true"/> if potential IPv4 address otherwise <see langword="false"/></returns>
    internal static bool IsPotentialIPv4Address(string potentialIPv4Address)
    {
        var parts = potentialIPv4Address.Split('.');

        if (parts.Length != 4)
            return false;

        foreach (var part in parts)
        {
            foreach (var character in part)
            {
                if (!char.IsAsciiDigit(character))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines if a string is a potential IPv6 address.
    /// </summary>
    /// <param name="potentialIPv6Address">The potential IPv6 address.</param>
    /// <returns><see langword="true"/> if potential IPv6 address otherwise <see langword="false"/></returns>
    internal static bool IsPotentialIPv6Address(string potentialIPv6Address)
    {
        var parts = potentialIPv6Address.Split(':');

        if (parts.Length < 3 ||
            parts.Length > 8)
        {
            return false;
        }

        foreach (var part in parts)
        {
            foreach (var character in part)
            {
                if (!char.IsAsciiDigit(character) &&
                    !char.IsAsciiLetter(character))
                {
                    return false;
                }
            }
        }

        return true;
    }
}

