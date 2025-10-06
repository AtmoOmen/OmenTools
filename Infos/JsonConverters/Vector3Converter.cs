using Newtonsoft.Json;
using System.Numerics;

namespace OmenTools.Infos;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartObject)
            throw new JsonException("Expected start of object");

        float x = 0, y = 0, z = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject)
                return new Vector3(x, y, z);

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
                case "z":
                    z = Convert.ToSingle(reader.Value);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Expected end of object");
    }

    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.X);
        writer.WritePropertyName("y");
        writer.WriteValue(value.Y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.Z);
        writer.WriteEndObject();
    }
}
