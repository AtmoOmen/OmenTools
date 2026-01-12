using Newtonsoft.Json;
using System.Numerics;

namespace OmenTools.Infos;

public class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartObject)
            throw new JsonException("Expected start of object");

        float x = 0, y = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject)
                return new Vector2(x, y);

            if (reader.TokenType != JsonToken.PropertyName)
                throw new JsonException("Expected property name");

            var propertyName = reader.Value?.ToString();
            reader.Read();

            switch (propertyName?.ToLower())
            {
                case "x":
                    x = Convert.ToSingle(reader.Value);
                    break;
                case "y":
                    y = Convert.ToSingle(reader.Value);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Expected end of object");
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.X);
        writer.WritePropertyName("y");
        writer.WriteValue(value.Y);
        writer.WriteEndObject();
    }
}
