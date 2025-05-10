using Newtonsoft.Json;

namespace OmenTools.Infos;

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer) => 
        DateTime.Parse((string?)reader.Value ?? string.Empty);

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer) => 
        writer.WriteValue(value.ToString("o"));
}
