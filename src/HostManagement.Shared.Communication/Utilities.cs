namespace HostManagement.Shared.Communication;

/// <summary>
/// Utility functions.
/// </summary>
public static class Utilities
{
    /// <summary>
    /// Converts a byte array to a hex string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The hex string.</returns>
    public static string BytesToHexString(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
            return string.Empty;

        // Allocate an array twice the size of the input array,
        // to store two hexadecimal characters per byte.
        var hexCharacters = new char[bytes.Length * 2];

        for (var i = 0; i < bytes.Length; i++)
        {
            /*
             * The upper nibble (the upper 4 bits) is converted to the first hex character.
             * 
             * 1. (nibble - 10)
             *    -> The result is either negative (for 0–9) or positive/null (for 10–15)
             * 
             * 2. ((nibble - 10) >> 31)
             *    -> Shift 31 bits to the right -> for 'int' this is the sign bit
             *    -> The result is either '-1' (for 0–9) or '0' (for 10-15)
             *
             * 3. ((nibble - 10) >> 31) & -7
             *    -> The result is either '-7' (for 0–9) or '0' (for 10–15)
             *
             * 4. 55 + nibble + ...
             *    -> This adds 55 to the nibble, which is the ASCII offset for '0' (48) and 'A' (65)
             *    -> The result is either '0' to '9' (for 0-9) or 'A' to 'F' (for 10-15)
             */
            var nibble = bytes[i] >> 4;
            hexCharacters[i * 2] = (char)(55 + nibble + (((nibble - 10) >> 31) & -7));

            // The lower nibble (the lower 4 bits) is converted to the second hex character.
            nibble = bytes[i] & 0xF;
            hexCharacters[(i * 2) + 1] = (char)(55 + nibble + (((nibble - 10) >> 31) & -7));
        }

        return new string(hexCharacters);
    }

    /// <summary>
    /// Gets varint bytes for the given number.
    /// </summary>
    /// <remarks>See <see href="https://protobuf.dev/programming-guides/encoding/#varints">
    /// https://protobuf.dev/programming-guides/encoding/#varints</see></remarks>
    /// <param name="number">The number to encode.</param>
    /// <returns>Bytes for the given number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is negative
    /// (varints are supposed to be unsigned).</exception>
    public static byte[] GetVarInt(long number)
    {
        if (number < 0)
            throw new ArgumentOutOfRangeException(nameof(number), "Cannot write negative value as varint.");

        var bytes = new List<byte>();

        while (true)
        {
            // Find the least significant 7 bits of the number
            var sizeByteAmount = number & 0x7f;
            var sizeByte = (byte)sizeByteAmount;

            // If after right shifting the number is greater than 0,
            // it means we have bits left. So we need to add a continuation bit (0x80).
            number >>= 7;
            var useContinuationBit = number > 0;

            if (useContinuationBit)
                sizeByte += 0x80;

            bytes.Add(sizeByte);

            if (!useContinuationBit)
                break;
        }

        return [.. bytes];
    }
}
