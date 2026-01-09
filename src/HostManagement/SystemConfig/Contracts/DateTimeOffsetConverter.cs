using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

internal sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc/>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.String or JsonTokenType.Null)
        {
            DateTimeOffset value = default;
            var text = reader.GetString();
            if (text is not null)
                value = DateTimeOffset.Parse(text);
            return value;
        }
        else
        {
            throw new JsonException("Expected string or null as the JSON token type.");
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("O"));
}
