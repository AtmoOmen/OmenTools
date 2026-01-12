using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record GitHubRelease
{
    [JsonProperty("id")]           public int               ID          { get; set; }
    [JsonProperty("tag_name")]     public string            TagName     { get; set; } = null!;
    [JsonProperty("name")]         public string            Name        { get; set; } = null!;
    [JsonProperty("body")]         public string            Body        { get; set; } = null!;
    [JsonProperty("published_at")] public DateTime          PublishedAt { get; set; }
    [JsonProperty("assets")]       public List<GitHubAsset> Assets      { get; set; } = null!;
}
