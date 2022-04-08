using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaceControl.Common.JsonConverter;

public class NullableIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return 0;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var valueAsString = reader.GetString();

            if (int.TryParse(valueAsString, out var val))
            {
                return val;
            }

            return 0;
        }

        if (reader.TryGetInt32(out var value))
        {
            return value;
        }

        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        // We don't need to implement the method as we don't have to convert the type to JSON
        throw new NotImplementedException();
    }
}