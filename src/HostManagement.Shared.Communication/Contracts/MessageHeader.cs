using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the header of a framed named pipe message.
/// </summary>
/// <remarks>
/// A frame is the varint-encoded header length, the JSON-serialized header, the varint-encoded payload length and
/// the payload bytes. Keeping the payload outside the header means it is written verbatim instead of being escaped
/// into the header's JSON, which is what a single JSON envelope containing the content as a string would do.
/// </remarks>
public sealed class MessageHeader
{
    /// <summary>
    /// Unique identifier for the conversation to correlate responses to requests.
    /// </summary>
    public Guid ConversationId { get; init; }

    /// <summary>
    /// The topic of the message used to identify a message handler (see <see cref="Topics"/>).
    /// </summary>
    public required string Topic { get; init; }

    /// <summary>
    /// The <see cref="MessageType"/>.
    /// </summary>
    public required MessageType Type { get; init; }
}
