using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSActivityCalendar
{
    [JsonProperty("code")] public int                          Code    { get; set; }
    [JsonProperty("msg")]  public string                       Message { get; set; } = null!;
    [JsonProperty("data")] public List<RSActivityCalendarData> Data    { get; set; } = null!;
}
