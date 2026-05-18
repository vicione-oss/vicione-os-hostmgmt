using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents a named pipe message.
/// </summary>
public class NamedPipeMessage
{
    /// <summary>
    /// Message content (can be JSON serialized) or empty.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the conversation to correlate responses to requests.
    /// </summary>
    public Guid ConversationId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The topic of the message used to identify a message handler (see <see cref="Topics"/>).
    /// </summary>
    public required string Topic { get; init; }

    /// <summary>
    /// The <see cref="MessageType"/>.
    /// </summary>
    public required MessageType Type { get; set; }
}
