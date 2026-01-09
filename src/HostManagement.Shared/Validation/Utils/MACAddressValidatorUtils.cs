using static System.MemoryExtensions;

namespace HostManagement.Shared.Validation.Utils;

/// <summary>
/// Provides utility methods to check a MAC address.
/// </summary>
internal static class MACAddressValidatorUtils
{
    private const int OctetCount = 6;
    private const int OctetLength = 2;

    internal static ReadOnlySpan<char> Separators => ":-";

    /// <summary>
    /// Determines if a MAC address has no separators.
    /// </summary>
    /// <param name="address">The MAC address.</param>
    /// <returns><see langword="true"/> if address has no separator, otherwise <see langword="false"/></returns>
    internal static bool HasNoSeparators(ReadOnlySpan<char> address) => !address.Contains(':') && !address.Contains('-');

    /// <summary>
    /// Determines if a MAC address has invalid octet count (less or greater than 6).
    /// </summary>
    /// <param name="octets">The octets of the MAC address.</param>
    /// <returns><see langword="true"/> if address has invalid octet count, otherwise <see langword="false"/></returns>
    internal static bool HasInvalidOctetCount(SpanSplitEnumerator<char> octets)
    {
        var count = 0;

        foreach (var _ in octets)
            count++;

        return count != OctetCount;
    }

    /// <summary>
    /// Determines if an octet has invalid length (less or greater than 2).
    /// </summary>
    /// <param name="octet">The octet.</param>
    /// <returns><see langword="true"/> if octet has invalid length, otherwise <see langword="false"/></returns>
    internal static bool HasOctetInvalidLength(ReadOnlySpan<char> octet) => octet.Length != OctetLength;

    /// <summary>
    /// Determines if a value has a non ASCII hexadecimal digit.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see langword="true"/> if non ASCII hexadecimal digit found, otherwise <see langword="false"/></returns>
    internal static bool HasNonAsciiHexDigit(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (!char.IsAsciiHexDigit(character))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the least significant bit (LSB - bit 0) of the specified byte is set.
    /// </summary>
    /// <param name="byteValue">The byte value.</param>
    /// <returns><see langword="true"/> if the LSB is set; otherwise <see langword="false"/>.</returns>
    internal static bool IsLeastSignificantBitSet(byte byteValue) => (byteValue & 0x01) == 0x01;
}
