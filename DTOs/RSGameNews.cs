using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSGameNews
{
    [JsonProperty("code")]    public int                  Code    { get; set; }
    [JsonProperty("message")] public string               Message { get; set; } = null!;
    [JsonProperty("data")]    public List<RSGameNewsData> Data    { get; set; } = null!;
}
