using Newtonsoft.Json;

namespace OmenTools.Infos;

public class VersionConverter : JsonConverter<Version>
{
    public override Version ReadJson(JsonReader reader, Type objectType, Version? existingValue, bool hasExistingValue, JsonSerializer serializer) => Version.Parse((string?)reader.Value ?? string.Empty);

    public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }
}
