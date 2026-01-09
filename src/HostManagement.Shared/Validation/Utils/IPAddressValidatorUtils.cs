using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace HostManagement.Shared.Validation.Utils;

/// <summary>
/// Provides utility methods to determine if an IPv4 or IPv6 address is link-local, multicast etc.
/// </summary>
internal static class IPAddressValidatorUtils
{
    private static readonly IPNetwork s_ipv4NotRoutableNetwork = IPNetwork.Parse("0.0.0.0/32");

    private static readonly IPNetwork s_ipv4LoopbackNetwork = IPNetwork.Parse("127.0.0.0/8");
    private static readonly IPNetwork s_ipv6LoopbackNetwork = IPNetwork.Parse("::1/128");

    private static readonly IPNetwork s_ipv4LinkLocalNetwork = IPNetwork.Parse("169.254.0.0/16");
    private static readonly IPNetwork s_ipv6LinkLocalNetwork = IPNetwork.Parse("fe80::/10");

    private static readonly IPNetwork s_ipv4MulticastNetwork = IPNetwork.Parse("224.0.0.0/4");
    private static readonly IPNetwork s_ipv6MulticastNetwork = IPNetwork.Parse("ff00::/8");

    private static readonly IPNetwork s_ipv4BroadcastNetwork = IPNetwork.Parse("255.255.255.255/32");

    /// <summary>
    /// A string representing the IPv4 'not routable' network in CIDR notation ("0.0.0.0/32").
    /// </summary>
    internal static string IPv4NotRoutableNetworkCIDR { get; } = s_ipv4NotRoutableNetwork.ToString();

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
    /// Determines if an IPv4 address is a not routable address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <returns><see langword="true"/> if not routable otherwise <see langword="false"/></returns>
    internal static bool IsIPv4NotRoutable(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
            return false;

        return s_ipv4NotRoutableNetwork.Contains(address);
    }

    /// <summary>
    /// Determines if an IPv4 address is a not routable address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <param name="notRoutableCIDRString">A string representing the IPv4 not routable network
    /// in CIDR notation.</param>
    /// <returns><see langword="true"/> if not routable otherwise <see langword="false"/></returns>
    internal static bool IsNotRoutable(IPAddress address, [NotNullWhen(true)] out string? notRoutableCIDRString)
    {
        notRoutableCIDRString = null;

        if (address.AddressFamily == AddressFamily.InterNetwork && IsIPv4NotRoutable(address))
        {
            notRoutableCIDRString = IPv4NotRoutableNetworkCIDR;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an IPv4 address is a loopback address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <returns><see langword="true"/> if loopback otherwise <see langword="false"/></returns>
    internal static bool IsIPv4Loopback(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
            return false;

        return s_ipv4LoopbackNetwork.Contains(address);
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

        return s_ipv6LoopbackNetwork.Contains(address);
    }

    /// <summary>
    /// Determines if an IPv4 or IPv6 address is a loopback address.
    /// </summary>
    /// <param name="address">The IPv4 or IPv6 address.</param>
    /// <param name="loopbackCIDRString">A string representing the IPv4 or IPv6 loopback network
    /// in CIDR notation.</param>
    /// <returns><see langword="true"/> if loopback otherwise <see langword="false"/></returns>
    internal static bool IsLoopback(IPAddress address, [NotNullWhen(true)] out string? loopbackCIDRString)
    {
        loopbackCIDRString = null;

        if (address.AddressFamily == AddressFamily.InterNetwork && IsIPv4Loopback(address))
        {
            loopbackCIDRString = IPv4LoopbackNetworkCIDR;
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6 && IsIPv6Loopback(address))
        {
            loopbackCIDRString = IPv6LoopbackNetworkCIDR;
            return true;
        }

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

        return s_ipv4LinkLocalNetwork.Contains(address);
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

        return s_ipv6LinkLocalNetwork.Contains(address);
    }

    /// <summary>
    /// Determines if an IPv4 or IPv6 address is a link-local address.
    /// </summary>
    /// <param name="address">The IPv4 or IPv6 address.</param>
    /// <param name="linkLocalCIDRString">A string representing the IPv4 or IPv6 link-local network
    /// in CIDR notation.</param>
    /// <returns><see langword="true"/> if link-local otherwise <see langword="false"/></returns>
    internal static bool IsLinkLocal(IPAddress address, [NotNullWhen(true)] out string? linkLocalCIDRString)
    {
        linkLocalCIDRString = null;

        if (address.AddressFamily == AddressFamily.InterNetwork && IsIPv4LinkLocal(address))
        {
            linkLocalCIDRString = IPv4LinkLocalNetworkCIDR;
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6 && IsIPv6LinkLocal(address))
        {
            linkLocalCIDRString = IPv6LinkLocalNetworkCIDR;
            return true;
        }

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

        return s_ipv4MulticastNetwork.Contains(address);
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

        return s_ipv6MulticastNetwork.Contains(address);
    }

    /// <summary>
    /// Determines if an IPv4 or IPv6 address is a multicast address.
    /// </summary>
    /// <param name="address">The IPv4 or IPv6 address.</param>
    /// <param name="multicastCIDRString">A string representing the IPv4 or IPv6 multicast network
    /// in CIDR notation.</param>
    /// <returns><see langword="true"/> if multicast otherwise <see langword="false"/></returns>
    internal static bool IsMulticast(IPAddress address, [NotNullWhen(true)] out string? multicastCIDRString)
    {
        multicastCIDRString = null;

        if (address.AddressFamily == AddressFamily.InterNetwork && IsIPv4Multicast(address))
        {
            multicastCIDRString = IPv4MulticastNetworkCIDR;
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6 && IsIPv6Multicast(address))
        {
            multicastCIDRString = IPv6MulticastNetworkCIDR;
            return true;
        }

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

        return s_ipv4BroadcastNetwork.Contains(address);
    }

    /// <summary>
    /// Determines if an IPv4 address is a broadcast address.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <param name="broadcastCIDRString">A string representing the IPv4 broadcast network in CIDR notation.</param>
    /// <returns><see langword="true"/> if broadcast otherwise <see langword="false"/></returns>
    internal static bool IsBroadcast(IPAddress address, [NotNullWhen(true)] out string? broadcastCIDRString)
    {
        broadcastCIDRString = null;

        if (address.AddressFamily == AddressFamily.InterNetwork && IsIPv4Broadcast(address))
        {
            broadcastCIDRString = IPv4BroadcastNetworkCIDR;
            return true;
        }

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

        if (parts.Length is < 3 or > 8)
            return false;

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

    /// <summary>
    /// Determines if an IP address is in a different subnet than another IP address,
    /// i.e. has a different network address.
    /// </summary>
    /// <remarks>The network address is the first address in a subnet.</remarks>
    /// <param name="firstAddress">The first IP address.</param>
    /// <param name="secondAddress">The second IP address.</param>
    /// <param name="netmask">The netmask.</param>
    /// <param name="firstNetworkAddressString">A string representing the first network address.</param>
    /// <param name="secondNetworkAddressString">A string representing the second network address.</param>
    /// <returns><see langword="true"/> if in a different subnet otherwise <see langword="false"/></returns>
    internal static bool IsInDifferentSubnet(
        IPAddress firstAddress,
        IPAddress secondAddress,
        IPAddress netmask,
        [NotNullWhen(true)] out string? firstNetworkAddressString,
        [NotNullWhen(true)] out string? secondNetworkAddressString)
    {
        firstNetworkAddressString = null;
        secondNetworkAddressString = null;

        var firstNetworkAddress = GetNetworkAddress(firstAddress, netmask);
        var secondNetworkAddress = GetNetworkAddress(secondAddress, netmask);

        if (!firstNetworkAddress.Equals(secondNetworkAddress))
        {
            firstNetworkAddressString = firstNetworkAddress.ToString();
            secondNetworkAddressString = secondNetworkAddress.ToString();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an IP address is equal to the network address of the subnet specified by
    /// <paramref name="addressToCheckAgainst"/> and <paramref name="netmaskToCheckAgainst"/>.
    /// </summary>
    /// <remarks>The network address is the first address in a subnet.</remarks>
    /// <param name="address">The IP address to check.</param>
    /// <param name="addressToCheckAgainst">The IP address to check against.</param>
    /// <param name="netmaskToCheckAgainst">The netmask to check against.</param>
    /// <param name="networkAddressString">A string representing the network address.</param>
    /// <returns><see langword="true"/> if the address is the network address otherwise <see langword="false"/></returns>
    internal static bool IsSubnetNetworkAddress(
        IPAddress address,
        IPAddress addressToCheckAgainst,
        IPAddress netmaskToCheckAgainst,
        [NotNullWhen(true)] out string? networkAddressString)
    {
        networkAddressString = null;

        var networkAddress = GetNetworkAddress(addressToCheckAgainst, netmaskToCheckAgainst);

        if (address.Equals(networkAddress))
        {
            networkAddressString = networkAddress.ToString();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an IP address is equal to the broadcast address of the subnet specified by
    /// <paramref name="addressToCheckAgainst"/> and <paramref name="netmaskToCheckAgainst"/>.
    /// </summary>
    /// <remarks>The broadcast address is the last address in a subnet.</remarks>
    /// <param name="address">The IP address to check.</param>
    /// <param name="addressToCheckAgainst">The IP address to check against.</param>
    /// <param name="netmaskToCheckAgainst">The netmask to check against.</param>
    /// <param name="broadcastAddressString">A string representing the broadcast address.</param>
    /// <returns><see langword="true"/> if the address is the broadcast address otherwise <see langword="false"/></returns>
    internal static bool IsSubnetBroadcastAddress(
        IPAddress address,
        IPAddress addressToCheckAgainst,
        IPAddress netmaskToCheckAgainst,
        [NotNullWhen(true)] out string? broadcastAddressString)
    {
        broadcastAddressString = null;

        var broadcastAddress = GetBroadcastAddress(addressToCheckAgainst, netmaskToCheckAgainst);

        if (address.Equals(broadcastAddress))
        {
            broadcastAddressString = broadcastAddress.ToString();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an IP address is a host address in the subnet specified by
    /// <paramref name="address"/> and <paramref name="netmask"/>.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="netmask">The netmask.</param>
    /// <param name="availableHostsString">A string representing the available hosts.</param>
    /// <returns></returns>
    internal static bool IsSubnetHostAddress(
        IPAddress address,
        IPAddress netmask,
        out string availableHostsString)
    {
        var networkAddress = GetNetworkAddress(address, netmask);
        var broadcastAddress = GetBroadcastAddress(address, netmask);

        var networkAddressAsUInt = GetIPv4AddressAsUInt(networkAddress);
        var broadcastAddressAsUInt = GetIPv4AddressAsUInt(broadcastAddress);

        if (AreHostsAvailable(networkAddressAsUInt, broadcastAddressAsUInt, out availableHostsString))
        {
            var addressAsUInt = GetIPv4AddressAsUInt(address);

            return
                addressAsUInt > networkAddressAsUInt &&
                addressAsUInt < broadcastAddressAsUInt;
        }

        return false;
    }

    /// <summary>
    /// Gets the IPv4 address as a <see cref="uint"/>.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns><see cref="uint"/></returns>
    /// <exception cref="ArgumentException"></exception>
    internal static uint GetIPv4AddressAsUInt(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
            throw new ArgumentException("IP address must be an IPv4 address.");

        var bytes = address.GetAddressBytes();

        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return BitConverter.ToUInt32(bytes, 0);
    }

    private static IPAddress GetNetworkAddress(IPAddress address, IPAddress netmask)
    {
        if (IsDifferentAddressFamily(address, netmask))
            throw new ArgumentException("All IP addresses must be of the same IP family.");

        var addressBytes = address.GetAddressBytes();
        var netmaskBytes = netmask.GetAddressBytes();

        var result = new byte[addressBytes.Length];

        for (var i = 0; i < addressBytes.Length; i++)
        {
            // Perform a bitwise logical AND operation between address and netmask
            result[i] = (byte)(addressBytes[i] & netmaskBytes[i]);
        }

        return new IPAddress(result);
    }

    private static IPAddress GetBroadcastAddress(IPAddress address, IPAddress netmask)
    {
        if (IsDifferentAddressFamily(address, netmask))
            throw new ArgumentException("All IP addresses must be of the same IP family.");

        var addressBytes = address.GetAddressBytes();
        var netmaskBytes = netmask.GetAddressBytes();

        var result = new byte[addressBytes.Length];

        for (var i = 0; i < addressBytes.Length; i++)
        {
            // Perform a bitwise logical OR operation between address and inverted netmask
            result[i] = (byte)(addressBytes[i] | ~netmaskBytes[i]);
        }

        return new IPAddress(result);
    }

    private static bool AreHostsAvailable(
        uint networkAddressAsUInt,
        uint broadcastAddressAsUInt,
        out string availableHostsString)
    {
        // Cast to long to handle negative intermediate results correctly and avoid overflow
        var hostCount = (long)broadcastAddressAsUInt - networkAddressAsUInt - 1;

        if (hostCount <= 0)
        {
            availableHostsString = "0";
            return false;
        }

        var firstHostAsUInt = networkAddressAsUInt + 1;
        var lastHostAsUInt = broadcastAddressAsUInt - 1;

        var firstHostAddress = new IPAddress(
            BitConverter.GetBytes(
                BitConverter.IsLittleEndian
                ? IPAddress.HostToNetworkOrder((int)firstHostAsUInt)
                : (int)firstHostAsUInt));

        var lastHostAddress = new IPAddress(
            BitConverter.GetBytes(
                BitConverter.IsLittleEndian
                ? IPAddress.HostToNetworkOrder((int)lastHostAsUInt)
                : (int)lastHostAsUInt));

        availableHostsString = $"{firstHostAddress} - {lastHostAddress}";
        return true;
    }

    private static bool IsDifferentAddressFamily(IPAddress firstAddress, IPAddress secondAddress)
        => firstAddress.AddressFamily != secondAddress.AddressFamily;
}
