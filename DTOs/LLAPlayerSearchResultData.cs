using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record LLAPlayerSearchResultData
{
    [JsonProperty("id")]      public int    CharacterID   { get; set; }
    [JsonProperty("name")]    public string CharacterName { get; set; } = null!;
    [JsonProperty("iconUrl")] public string IconURL       { get; set; } = null!;
    [JsonProperty("worldId")] public uint   WorldID       { get; set; }
}
