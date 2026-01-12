using Newtonsoft.Json;

namespace OmenTools.DTOs;

// 石之家玩家搜索结果
public record RSPlayerSearchResult
{
    [JsonProperty("code")] public int                            Code    { get; set; }
    [JsonProperty("msg")]  public string                         Message { get; set; } = null!;
    [JsonProperty("data")] public List<RSPlayerSearchResultData> Data    { get; set; } = null!;
}
