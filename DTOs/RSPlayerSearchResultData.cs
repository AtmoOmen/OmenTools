using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerSearchResultData
{
    [JsonProperty("uuid")]                 public uint   UUID               { get; set; }
    [JsonProperty("avatar")]               public string Avatar             { get; set; } = null!;
    [JsonProperty("character_name")]       public string CharacterName      { get; set; } = null!;
    [JsonProperty("area_name")]            public string AreaName           { get; set; } = null!;
    [JsonProperty("group_name")]           public string GroupName          { get; set; } = null!;
    [JsonProperty("profile")]              public string Profile            { get; set; } = null!;
    [JsonProperty("test_limited_badge")]   public uint   TestLimitedBadge   { get; set; }
    [JsonProperty("posts2_creator_badge")] public uint   Posts2CreatorBadge { get; set; }
    [JsonProperty("admin_tag")]            public int    AdminTag           { get; set; }
    [JsonProperty("fansNum")]              public uint   FansNum            { get; set; }
}
