using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

internal sealed class PhysicalAddressConverter : JsonConverter<PhysicalAddress>
{
    /// <inheritdoc/>
    public override PhysicalAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
            throw new JsonException("Expected string as the JSON token type.");

        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            return PhysicalAddress.None;

        if (value.Length is not 12 and not 17 ||
            !PhysicalAddress.TryParse(value.Replace('-', ':'), out var physicalAddress))
        {
            throw new JsonException($"Invalid physical address format: '{value}'");
        }

        return physicalAddress;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, PhysicalAddress value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToColonNotation());
}
