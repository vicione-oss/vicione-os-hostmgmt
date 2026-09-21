using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents an <see cref="IPAddressConverter"/>.
/// </summary>
public class IPAddressConverter : JsonConverter<IPAddress>
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsAssignableFrom(typeof(IPAddress));

    /// <inheritdoc/>
    public override IPAddress Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
            throw new JsonException("Expected string as the JSON token type.");

        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException($"IPAddress '{value}' can not be empty.");

        if (IPAddress.TryParse(value, out var ipAddress))
            return ipAddress;
        else
            throw new JsonException($"Invalid IP address format: '{value}'");
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        IPAddress value,
        JsonSerializerOptions options)
    {
        var ipAddressString = value is null
            ? string.Empty
            : value.ToString();

        writer.WriteStringValue(ipAddressString);
    }
}
