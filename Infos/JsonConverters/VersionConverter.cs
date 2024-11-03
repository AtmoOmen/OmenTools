using System.Text.Json;
using System.Text.Json.Serialization;

namespace OmenTools.Infos;

public class VersionConverter : JsonConverter<Version>
{
    public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
        => Version.Parse(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}