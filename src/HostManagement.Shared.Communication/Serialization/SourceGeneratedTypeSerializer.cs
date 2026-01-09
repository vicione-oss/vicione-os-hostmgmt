using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.Serialization;

/// <summary>
/// Provides functionality to serialize source generated types to JSON and to
/// deserialize JSON into those types (see <see cref="SourceGenerationContext"/>).
/// </summary>
public static class SourceGeneratedTypeSerializer
{
    /// <summary>
    /// Reads the UTF-8 encoded text representing a single JSON value into a <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name = "TValue" > The type to deserialize the JSON value into.</typeparam>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
    /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static TValue Deserialize<TValue>(
        string json,
        JsonTypeInfo<TValue> jsonTypeInfo)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            stream.Position = 0;

            var result = JsonSerializer.Deserialize(stream, jsonTypeInfo)
                ?? throw new InvalidOperationException(
                    $"JSON string could not be deserialized to {nameof(jsonTypeInfo.Type)}" +
                    Environment.NewLine +
                    json);

            return result;
        }
    }

    /// <summary>
    /// Converts the provided value into a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
    /// <returns>A string representation of the value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static string Serialize<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo)
        => JsonSerializer.Serialize(value, jsonTypeInfo);
}
