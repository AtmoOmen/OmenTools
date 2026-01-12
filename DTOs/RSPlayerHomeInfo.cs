using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerHomeInfo
{
    [JsonProperty("code")] public int                   Code    { get; set; }
    [JsonProperty("msg")]  public string?               Message { get; set; }
    [JsonProperty("data")] public RSPlayerHomeInfoData? Data    { get; set; }
}
