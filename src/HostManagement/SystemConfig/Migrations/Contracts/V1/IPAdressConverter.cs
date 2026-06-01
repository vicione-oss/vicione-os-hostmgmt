using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

internal sealed class IPAddressConverter : JsonConverter<IPAddress>
{
    /// <inheritdoc/>
    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.String or JsonTokenType.Null)
        {
            IPAddress? value = null;
            var text = reader.GetString();
            if (text is not null)
                value = IPAddress.Parse(text);
            return value;
        }
        else
        {
            throw new JsonException("Expected string or null as the JSON token type.");
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
