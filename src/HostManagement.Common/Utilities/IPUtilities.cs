using System.Net;
using System.Net.Sockets;

namespace HostManagement.Common.Utilities;

/// <summary>
/// Provides utility methods for converting between IPv4 or IPv6 addresses and their CIDR notation.
/// </summary>
public static class IPUtilities
{
    /// <summary>
    /// Converts an IPv4 or IPv6 address and netmask to CIDR notation.
    /// </summary>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="netmask">The netmask.</param>
    /// <returns>CIDR notation of the specified IP address and netmask</returns>
    public static string ConvertToCIDR(IPAddress ipAddress, IPAddress netmask)
    {
        return $"{ipAddress}/{GetCIDRLengthFromNetmask(netmask)}";
    }

    /// <summary>
    /// Gets the CIDR length from an IPv4 or IPv6 netmask.
    /// </summary>
    /// <param name="netmask">The netmask.</param>
    /// <returns>CIDR length representing the IPv4 or IPv6 netmask</returns>
    public static int GetCIDRLengthFromNetmask(IPAddress netmask)
    {
        var netmaskBytes = netmask.GetAddressBytes();
        var length = 0;

        foreach (var netmaskByte in netmaskBytes)
            length += CountBits(netmaskByte);

        return length;
    }

    /// <summary>
    /// Converts a CIDR notation to an IPv4 or IPv6 address and corresponding netmask.
    /// </summary>
    /// <param name="cidr">The CIDR notation.</param>
    /// <returns>IP address and netmask</returns>
    /// <exception cref="FormatException"/>
    public static (IPAddress ipAddress, IPAddress netmask) ConvertFromCIDR(string cidr)
    {
        var parts = cidr.Split('/');

        if (!IPAddress.TryParse(parts[0], out var ipAddress))
            throw new FormatException("Could not parse IP address of provided CIDR string.");

        if (!int.TryParse(parts[1], out var netmaskLength))
            throw new FormatException("Could not parse netmask of provided CIDR string.");

        return (ipAddress, GetNetmaskFromCIDRLength(netmaskLength, ipAddress.AddressFamily));
    }

    /// <summary>
    /// Gets the IPv4 or IPv6 netmask from a CIDR length.
    /// </summary>
    /// <param name="cidrLength">The CIDR length.</param>
    /// <param name="addressFamily">The address family (IPv4 or IPv6).</param>
    /// <returns>IPv4 or IPv6 netmask representing the CIDR length</returns>
    public static IPAddress GetNetmaskFromCIDRLength(int cidrLength, AddressFamily addressFamily)
    {
        var maxCidrLength = addressFamily == AddressFamily.InterNetworkV6
            ? 128
            : 32;

        if (cidrLength < 0 ||
            cidrLength > maxCidrLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cidrLength),
                $"CIDR length must be between 0 and {maxCidrLength}.");
        }

        if (addressFamily == AddressFamily.InterNetwork)
        {
            // Initialize a 32-bit integer with all bits set to 1 and shift left to create the netmask
            var mask = cidrLength == 0
                ? 0
                : uint.MaxValue << (32 - cidrLength);

            var maskBytes = BitConverter.GetBytes(mask);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(maskBytes);

            return new IPAddress(maskBytes);
        }
        else if (addressFamily == AddressFamily.InterNetworkV6)
        {
            // Initialize a 128-bit (16-byte) array to represent the netmask
            var maskBytes = new byte[16];

            // Calculate the number of full bytes and remaining bits to set
            var fullBytes = cidrLength / 8;
            var remainingBits = cidrLength % 8;

            // Set the full bytes to 0xFF
            for (var i = 0; i < fullBytes; i++)
            {
                maskBytes[i] = 0xFF;
            }

            // Set the remaining bits in the next byte, if any
            if (remainingBits > 0)
                maskBytes[fullBytes] = (byte)(0xFF << (8 - remainingBits));

            return new IPAddress(maskBytes);
        }
        else
        {
            throw new ArgumentException("Unsupported address family.", nameof(addressFamily));
        }
    }

    /// <summary>
    /// Counts the number of '1' bits in a byte.
    /// </summary>
    private static int CountBits(byte b)
    {
        var count = 0;

        while (b != 0)
        {
            // Check the least significant bit
            count += b & 1;

            // Right shift the byte by 1 bit
            b >>= 1;
        }

        return count;
    }
}
