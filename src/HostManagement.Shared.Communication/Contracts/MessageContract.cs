namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Binds a topic to the message contracts exchanged on it, so callers do not have to name the contract types.
/// </summary>
/// <remarks>
/// <see cref="string"/> denotes raw text, which is sent and received unchanged
/// (see <see cref="HostManagement.Shared.Serialization.MessageSerializer"/>).
/// </remarks>
public abstract class MessageContract
{
    /// <summary>
    /// Initializes a new <see cref="MessageContract"/>.
    /// </summary>
    /// <param name="topic">The topic the contracts belong to.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="topic"/> is <see langword="null"/>, empty or whitespace.
    /// </exception>
    protected MessageContract(string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        Topic = topic;
    }

    /// <summary>
    /// The topic the contracts belong to.
    /// </summary>
    public string Topic { get; }

    /// <inheritdoc/>
    public override string ToString() => Topic;
}

/// <summary>
/// A request topic whose response is a <typeparamref name="TResponse"/> and whose request content is raw text.
/// </summary>
/// <typeparam name="TResponse">The response contract type.</typeparam>
/// <param name="topic">The topic of the request (see <see cref="Topics"/>).</param>
public sealed class TopicContract<TResponse>(string topic) : MessageContract(topic);

/// <summary>
/// A request topic whose request is a <typeparamref name="TRequest"/>
/// and whose response is a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The request contract type.</typeparam>
/// <typeparam name="TResponse">The response contract type.</typeparam>
/// <param name="topic">The topic of the request (see <see cref="Topics"/>).</param>
public sealed class TopicContract<TRequest, TResponse>(string topic) : MessageContract(topic);

/// <summary>
/// An event topic whose message is a <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The event message contract type.</typeparam>
/// <param name="topic">The topic of the event (see <see cref="EventTopics"/>).</param>
public sealed class EventContract<TMessage>(string topic) : MessageContract(topic);
