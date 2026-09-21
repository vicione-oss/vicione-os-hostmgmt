using System.Globalization;
using System.Net.NetworkInformation;

namespace HostManagement;

/// <summary>
/// Provides extension members for <see cref="PhysicalAddress"/>.
/// </summary>
internal static class PhysicalAddressExtensions
{
    /// <summary>
    /// Provides extension members for the specified physical address.
    /// </summary>
    /// <param name="address">The physical address.</param>
    extension(PhysicalAddress address)
    {
        /// <summary>
        /// Formats a physical address using colon-separated hexadecimal octets.
        /// </summary>
        /// <returns>The colon-separated physical address.</returns>
        internal string ToColonNotation()
            => string.Join(
                ':',
                address.GetAddressBytes().Select(static octet => octet.ToString("X2", CultureInfo.InvariantCulture)));
    }
}
