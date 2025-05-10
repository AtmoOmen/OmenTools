using System.Numerics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OmenTools.Infos;

public class Vector2Converter : JsonConverter<Vector2>
{
    private static readonly Regex Vector2StringRegex = new(@"^<(-?\d+),\s*(-?\d+)>$", RegexOptions.Compiled);

    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.String:
            {
                var stringValue = reader.Value?.ToString();
                var match       = Vector2StringRegex.Match(stringValue ?? string.Empty);

                if (match.Success)
                {
                    var x = float.Parse(match.Groups[1].Value);
                    var y = float.Parse(match.Groups[2].Value);
                    return new Vector2(x, y);
                }

                throw new JsonException($"Invalid Vector2 string format: {stringValue}. Expected format: \"<x, y>\"");
            }
            case JsonToken.StartObject:
            {
                float x = 0, y = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject) return new Vector2(x, y);

                    if (reader.TokenType != JsonToken.PropertyName) throw new JsonException("Expected property name");

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

                break;
            }
        }

        throw new JsonException("Expected string in format \"<x, y>\" or object with X and Y properties");
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) =>
        writer.WriteValue($"<{value.X}, {value.Y}>");
}
