using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Contracts;

namespace HostManagement.Shared.Serialization;

/// <summary>
/// Provides functionality to serialize a <see cref="SystemConfiguration"/> object to JSON and to deserialize JSON into
/// a <see cref="SystemConfiguration"/> object (see <see cref="SourceGenerationContext"/>).
/// </summary>
public static class SystemConfigurationSerializer
{
    /// <summary>
    /// Parses the text representing a single JSON value into a <see cref="SystemConfiguration"/>. It uses the
    /// specified <see cref="JsonTypeInfo"/> for the <see cref="SystemConfiguration"/> or the default context if null.
    /// </summary>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="jsonTypeInfo">Metadata about the <see cref="SystemConfiguration"/> to convert.</param>
    /// <returns>A <see cref="SystemConfiguration"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static SystemConfiguration Deserialize(string json, JsonTypeInfo<SystemConfiguration>? jsonTypeInfo = null)
    {
        jsonTypeInfo ??= SourceGenerationContext.Default.SystemConfiguration;

        return JsonSerializer
            .Deserialize(json, jsonTypeInfo)
            ?? throw new InvalidOperationException(
                $"JSON string could not be deserialized to '{nameof(SystemConfiguration)}'." +
                Environment.NewLine +
                $"JSON string was: '{json}'");
    }

    /// <summary>
    /// Reads the UTF-8 encoded text representing a single JSON value into a <see cref="SystemConfiguration"/>. The
    /// Stream will be read to completion. It uses the specified <see cref="JsonTypeInfo"/> for the
    /// <see cref="SystemConfiguration"/> or the default context if null.
    /// </summary>
    /// <param name="utf8Json">The UTF-8 <see cref="Stream"/> to read from.</param>
    /// <param name="jsonTypeInfo">Metadata about the <see cref="SystemConfiguration"/> to convert.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A <see cref="SystemConfiguration"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public static async ValueTask<SystemConfiguration> DeserializeAsync(
        Stream utf8Json,
        JsonTypeInfo<SystemConfiguration>? jsonTypeInfo = null,
        CancellationToken cancellationToken = default)
    {
        jsonTypeInfo ??= SourceGenerationContext.Default.SystemConfiguration;

        return await JsonSerializer
            .DeserializeAsync(utf8Json, jsonTypeInfo, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"UTF-8 JSON stream could not be deserialized to '{nameof(SystemConfiguration)}'.");
    }

    /// <summary>
    /// Converts the provided <see cref="SystemConfiguration"/> value into a <see cref="string"/>. It uses the
    /// specified <see cref="JsonTypeInfo"/> for the <see cref="SystemConfiguration"/> or the default context if null.
    /// </summary>
    /// <param name="configuration">The <see cref="SystemConfiguration"/> to convert.</param>
    /// <param name="jsonTypeInfo">Metadata about the <see cref="SystemConfiguration"/> to convert.</param>
    /// <returns>A string representation of the <see cref="SystemConfiguration"/> value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidCastException"></exception>
    public static string Serialize(SystemConfiguration configuration, JsonTypeInfo? jsonTypeInfo = null)
    {
        jsonTypeInfo ??= SourceGenerationContext.Default.SystemConfiguration;

        return JsonSerializer.Serialize(configuration, jsonTypeInfo);
    }

    /// <summary>
    /// Converts the provided <see cref="SystemConfiguration"/> value to UTF-8 encoded JSON text and writes it to the
    /// <paramref name="utf8Json"/> stream. It uses the specified <see cref="JsonTypeInfo"/> for the
    /// <see cref="SystemConfiguration"/> or the default context if null.
    /// </summary>
    /// <param name="utf8Json">The UTF-8 <see cref="Stream"/> to write to.</param>
    /// <param name="configuration">The <see cref="SystemConfiguration"/> to convert.</param>
    /// <param name="jsonTypeInfo">Metadata about the <see cref="SystemConfiguration"/> to convert.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public static async Task SerializeAsync(
        Stream utf8Json,
        SystemConfiguration configuration,
        JsonTypeInfo<SystemConfiguration>? jsonTypeInfo = null,
        CancellationToken cancellationToken = default)
    {
        jsonTypeInfo ??= SourceGenerationContext.Default.SystemConfiguration;

        await JsonSerializer
            .SerializeAsync(utf8Json, configuration, jsonTypeInfo, cancellationToken)
            .ConfigureAwait(false);
    }
}
