using Newtonsoft.Json;

namespace OmenTools.DTOs;

// 石之家玩家主页成就搜索结果
public record RSPlayerAchievementSearch
{
    [JsonProperty("code")] public int                                 Code    { get; set; }
    [JsonProperty("msg")]  public string                              Message { get; set; } = null!;
    [JsonProperty("data")] public List<RSPlayerAchievementSearchData> Data    { get; set; } = null!;
}
