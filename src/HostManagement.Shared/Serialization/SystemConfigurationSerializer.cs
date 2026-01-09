using System.Text;
using System.Text.Json;
using HostManagement.Shared.Contracts;

namespace HostManagement.Shared.Serialization;

/// <summary>
/// Provides functionality to serialize a <see cref="SystemConfiguration"/> object to JSON and to
/// deserialize JSON into a <see cref="SystemConfiguration"/> object (see <see cref="SourceGenerationContext"/>).
/// </summary>
public static class SystemConfigurationSerializer
{
    /// <summary>
    /// Parses the text representing a single JSON value into a <see cref="SystemConfiguration"/>.
    /// </summary>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A <see cref="SystemConfiguration"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public static async Task<SystemConfiguration> Deserialize(
        string json,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            stream.Position = 0;

            var result = await JsonSerializer
                .DeserializeAsync(stream, SourceGenerationContext.Default.SystemConfiguration, cancellationToken)
                .ConfigureAwait(false)
                    ?? throw new InvalidOperationException(
                        $"JSON string could not be deserialized to '{nameof(SystemConfiguration)}'." +
                        Environment.NewLine +
                        $"JSON string was: '{json}'");

            return result;
        }
    }

    /// <summary>
    /// Converts the provided <see cref="SystemConfiguration"/> value into a <see cref="string"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="SystemConfiguration"/> to convert.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A string representation of the <see cref="SystemConfiguration"/> value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public static async Task<string> Serialize(
        SystemConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();

        await JsonSerializer
            .SerializeAsync(
                memoryStream,
                configuration,
                SourceGenerationContext.Default.SystemConfiguration,
                cancellationToken)
            .ConfigureAwait(false);

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}
