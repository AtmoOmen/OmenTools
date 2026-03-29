using Newtonsoft.Json;

namespace OmenTools.Info.DTOs.GitHub;

public record GitHubAsset
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("download_count")]
    public int DownloadCount { get; set; }
}
