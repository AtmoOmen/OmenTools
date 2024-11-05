using Newtonsoft.Json;

namespace OmenTools.Infos;

public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer) => TimeSpan.Parse((string?)reader.Value ?? string.Empty);

    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString("c"));
    }
}
