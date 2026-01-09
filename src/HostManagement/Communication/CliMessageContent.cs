namespace HostManagement.Communication;

/// <summary>
/// Represents a CLI message content which wraps the real message content.
/// </summary>
internal sealed class CliMessageContent
{
    /// <summary>
    /// The message content.
    /// </summary>
    public required string MessageContent { get; init; }
}
