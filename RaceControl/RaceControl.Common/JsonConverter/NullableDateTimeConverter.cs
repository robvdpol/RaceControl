using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaceControl.Common.JsonConverter;

public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(DateTime?) == typeToConvert;
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDateTime(out var date))
        {
            return date;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        // We don't need to implement the method as we don't have to convert the type to JSON
        throw new NotImplementedException();
    }
}