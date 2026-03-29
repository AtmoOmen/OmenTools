using Newtonsoft.Json;

namespace OmenTools.Info.DTOs.Lalachievements;

// Lalachievements 玩家搜索结果
public record LLAPlayerSearchResult
{
    [JsonProperty("deployTime")]
    public long DeployTime { get; set; }

    [JsonProperty("results")]
    public List<LLAPlayerSearchResultData> Data { get; set; } = null!;
}
