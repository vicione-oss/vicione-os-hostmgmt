
namespace HostManagement.Shared.Communication.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> class.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Asynchronously reads a var int from a stream.
    /// </summary>
    /// <remarks>See https://protobuf.dev/programming-guides/encoding/#varints</remarks>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The read number.</returns>
    public static async Task<int> ReadVarInt(this Stream stream, CancellationToken cancellationToken)
    {
        var lengthReadBuffer = new byte[1];

        var size = 0;
        var sizeByteShift = 0;
        while (true)
        {
            var readBytes = await stream.ReadAsync(lengthReadBuffer.AsMemory(0, 1), cancellationToken).ConfigureAwait(false);
            if (readBytes == 0)
                return 0;
            int sizeByte = lengthReadBuffer[0];

            // Strip the continuation bit
            var sizeBytePayload = sizeByte & 0x7f;

            // Least significant bytes are given first, so we shift left
            // further as we continue to read size bytes.
            var sizeByteAmount = sizeBytePayload << sizeByteShift;

            // Add the 7 payload bytes to the size
            size += sizeByteAmount;

            if ((sizeByte & 0x80) > 0)
            {
                // The continuation bit is set. Keep on reading bytes to determine the size.
                sizeByteShift += 7;
            }
            else
            {
                // No continuation bit. We're done determining the size.
                break;
            }
        }

        return size;
    }
}
