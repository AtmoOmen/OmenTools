using Newtonsoft.Json;

namespace OmenTools.DTOs;

// Lalachievements 玩家搜索结果
public record LLAPlayerSearchResult
{
    [JsonProperty("deployTime")] public long                            DeployTime { get; set; }
    [JsonProperty("results")]    public List<LLAPlayerSearchResultData> Data       { get; set; } = null!;
}
