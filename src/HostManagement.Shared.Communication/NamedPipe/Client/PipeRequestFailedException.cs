using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a request that HostManagement answered with a plain <see cref="Response"/> instead of the expected
/// response contract, because the request failed on its side (for example no handler is registered for the topic,
/// or the handler threw or was canceled).
/// </summary>
/// <remarks>
/// The <see cref="Exception.Message"/> is the reason reported by HostManagement.
/// <para>
/// Derives from <see cref="InvalidOperationException"/>, so callers that catch the failures to connect or to send a
/// request also catch this one.
/// </para>
/// </remarks>
[Serializable]
public sealed class PipeRequestFailedException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new <see cref="PipeRequestFailedException"/>.
    /// </summary>
    public PipeRequestFailedException()
    { }

    /// <summary>
    /// Initializes a new <see cref="PipeRequestFailedException"/>.
    /// </summary>
    /// <inheritdoc/>
    public PipeRequestFailedException(string? message)
        : base(message)
    { }

    /// <summary>
    /// Initializes a new <see cref="PipeRequestFailedException"/>.
    /// </summary>
    /// <inheritdoc/>
    public PipeRequestFailedException(string? message, Exception? innerException)
        : base(message, innerException)
    { }

    /// <summary>
    /// Initializes a new <see cref="PipeRequestFailedException"/> for a failed request.
    /// </summary>
    /// <param name="topic">The topic of the failed request.</param>
    /// <param name="conversationId">The conversation ID of the failed request.</param>
    /// <param name="reason">The reason reported by HostManagement.</param>
    /// <param name="innerException">
    /// The exception thrown while deserializing the response content into the expected response contract.
    /// </param>
    public PipeRequestFailedException(string topic, Guid conversationId, string reason, Exception? innerException)
        : base(reason, innerException)
    {
        Topic = topic;
        ConversationId = conversationId;
    }

    /// <summary>
    /// The topic of the failed request, or <see langword="null"/> if it was not provided.
    /// </summary>
    public string? Topic { get; }

    /// <summary>
    /// The conversation ID of the failed request, or <see cref="Guid.Empty"/> if it was not provided.
    /// </summary>
    public Guid ConversationId { get; }
}
