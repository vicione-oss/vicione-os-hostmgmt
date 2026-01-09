namespace HostManagement.Shared.Validation.Utils;

/// <summary>
/// Provides utility methods to check a MAC address.
/// </summary>
internal static class MACAddressValidatorUtils
{
    internal static char[] s_separators = [':', '-'];

    /// <summary>
    /// Determines if a MAC address has no separators.
    /// </summary>
    /// <param name="address">The MAC address.</param>
    /// <returns><see langword="true"/> if address has no separator, otherwise <see langword="false"/></returns>
    internal static bool HasNoSeparators(string address)
    {
        if (!address.Contains(':') &&
            !address.Contains('-'))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if a MAC address has invalid octet count (less or greater than 6).
    /// </summary>
    /// <param name="address">The MAC address.</param>
    /// <returns><see langword="true"/> if address has invalid octet count, otherwise <see langword="false"/></returns>
    internal static bool HasInvalidOctetCount(string address)
    {
        var octets = address.Split(s_separators);

        if (HasInvalidOctetCount(octets))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if a MAC address has invalid octet count (less or greater than 6).
    /// </summary>
    /// <param name="octets">The octets of the MAC address.</param>
    /// <returns><see langword="true"/> if address has invalid octet count, otherwise <see langword="false"/></returns>
    internal static bool HasInvalidOctetCount(string[] octets)
    {
        if (octets.Length != 6)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if an octet has invalid length (less or greater than 2).
    /// </summary>
    /// <param name="octet">The octet.</param>
    /// <returns><see langword="true"/> if octet has invalid length, otherwise <see langword="false"/></returns>
    internal static bool HasOctetInvalidLength(string octet)
    {
        if (octet.Length != 2)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if an octet has a non ASCII hexadecimal digit.
    /// </summary>
    /// <param name="octet">The octet.</param>
    /// <returns><see langword="true"/> if non ASCII hexadecimal digit found, otherwise <see langword="false"/></returns>
    internal static bool HasOctetNonAsciiHexDigit(string octet)
    {
        if (HasNonAsciiHexDigit(octet))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if a MAC address without separators has a non ASCII hexadecimal digit.
    /// </summary>
    /// <param name="addressWithoutSeparators">The MAC address without separators.</param>
    /// <returns><see langword="true"/> if non ASCII hexadecimal digit found, otherwise <see langword="false"/></returns>
    internal static bool HasNonAsciiHexDigit(string addressWithoutSeparators)
    {
        foreach (var character in addressWithoutSeparators)
        {
            if (!char.IsAsciiHexDigit(character))
                return true;
        }

        return false;
    }
}
