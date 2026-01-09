namespace HostManagement.Common.Utilities;

/// <summary>
/// Provides utility methods for converting between colon and hyphen notation.
/// </summary>
public static class MACUtilities
{
    /// <summary>
    /// Converts a MAC address from hyphen to colon notation (e.g. aa:bb:cc:dd:ee:ff).
    /// </summary>
    /// <remarks>If the MAC address has no separators colons will be added.</remarks>
    /// <param name="macAddress">The MAC address.</param>
    /// <returns>The MAC address in colon notation.</returns>
    public static string ConvertToColonNotation(string macAddress)
    {
        if (HasNoSeparators(macAddress))
            return AddSeparators(macAddress, ':');
        else
            return macAddress.Replace('-', ':');
    }

    /// <summary>
    /// Converts a MAC address from colon to hyphen notation (e.g. aa-bb-cc-dd-ee-ff).
    /// </summary>
    /// <remarks>If the MAC address has no separators hyphens will be added.</remarks>
    /// <param name="macAddress">The MAC address.</param>
    /// <returns>The MAC address in hyphen notation.</returns>
    public static string ConvertToHyphenNotation(string macAddress)
    {
        if (HasNoSeparators(macAddress))
            return AddSeparators(macAddress, '-');
        else
            return macAddress.Replace(':', '-');
    }

    private static bool HasNoSeparators(string macAddress)
    {
        if (!macAddress.Contains(':') &&
            !macAddress.Contains('-'))
        {
            return true;
        }

        return false;
    }

    private static string AddSeparators(string macAddress, char separator)
    {
        return string.Join(
            separator,
            Enumerable
                .Range(0, macAddress.Length / 2)
                .Select(i => macAddress.Substring(i * 2, 2)));
    }
}
