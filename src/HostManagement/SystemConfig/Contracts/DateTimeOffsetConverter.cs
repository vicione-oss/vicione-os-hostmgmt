using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

internal sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc/>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
            throw new JsonException("Expected string or null as the JSON token type.");

        DateTimeOffset value = default;

        var text = reader.GetString();

        if (text is not null)
            value = DateTimeOffset.Parse(text);

        return value;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("O"));
}
