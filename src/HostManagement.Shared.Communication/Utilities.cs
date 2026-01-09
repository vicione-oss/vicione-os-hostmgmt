namespace HostManagement.Shared.Communication;

/// <summary>
/// Utility functions.
/// </summary>
public static class Utilities
{
    /// <summary>
    /// Convert a byte array to a hex string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The hex string.</returns>
    public static string BytesToHexString(byte[] bytes)
    {
        var c = new char[bytes.Length * 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = bytes[i] & 0xF;
            c[(i * 2) + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }
        return new string(c);
    }

    /// <summary>
    /// Gets varint bytes for the given number.
    /// </summary>
    /// <remarks>See https://protobuf.dev/programming-guides/encoding/#varints</remarks>
    /// <param name="value">The number to encode.</param>
    /// <returns>Bytes for the given number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value in negative (varints are supposed to be unsigned)</exception>
    public static byte[] GetVarInt(long value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot write negative value as varint");
        }

        var bytes = new List<byte>();

        while (true)
        {
            // Find the least significant 7 bits of the number
            var sizeByteAmount = value & 0x7f;

            value >>= 7;
            var sizeByte = (byte)sizeByteAmount;
            var useContinuationBit = value > 0;

            if (useContinuationBit)
            {
                // If there is remaining value left, add a continuation bit
                sizeByte += 0x80;
            }

            bytes.Add(sizeByte);

            if (!useContinuationBit)
            {
                break;
            }
        }

        return bytes.ToArray();
    }
}
