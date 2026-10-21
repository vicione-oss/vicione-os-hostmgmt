using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HostManagement.Shared.Serialization;

/// <summary>
/// Serializes and deserializes message contents without the caller having to name a
/// <see cref="JsonTypeInfo{T}"/>. The metadata is resolved through <see cref="MessageTypeRegistry"/>.
/// </summary>
/// <remarks>
/// <see cref="string"/> is passed through unchanged in both directions, so raw text messages
/// (for example the "echo" topic) keep their exact content and never get JSON quotes added.
/// </remarks>
public static class MessageSerializer
{
    /// <summary>
    /// Converts the provided value into its message content representation.
    /// </summary>
    /// <typeparam name="T">The message contract type.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>
    /// The JSON representation of <paramref name="value"/>,
    /// or the value itself if <typeparamref name="T"/> is <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no registered context provides metadata for <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when there is no compatible JsonConverter for <typeparamref name="T"/> or its serializable members.
    /// </exception>
    public static string Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (typeof(T) == typeof(string))
            return (string)(object)value;

        return JsonSerializer.Serialize(value, MessageTypeRegistry.GetTypeInfo<T>());
    }

    /// <summary>
    /// Converts the provided message content into a <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The message contract type.</typeparam>
    /// <param name="content">The message content to convert.</param>
    /// <returns>
    /// A <typeparamref name="T"/> representation of <paramref name="content"/>,
    /// or the content itself if <typeparamref name="T"/> is <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no registered context provides metadata for <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="JsonException">
    /// Thrown when <paramref name="content"/> is not valid JSON for <typeparamref name="T"/>,
    /// or when it deserializes to <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when there is no compatible JsonConverter for <typeparamref name="T"/> or its serializable members.
    /// </exception>
    public static T Deserialize<T>(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (typeof(T) == typeof(string))
            return (T)(object)content;

        return JsonSerializer.Deserialize(content, MessageTypeRegistry.GetTypeInfo<T>())
            ?? throw new JsonException(
                $"Message content could not be deserialized to '{typeof(T).Name}'." +
                Environment.NewLine +
                $"Message content was: '{content}'");
    }
}
