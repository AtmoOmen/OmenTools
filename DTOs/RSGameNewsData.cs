using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSGameNewsData
{
    [JsonProperty("id")]              public uint   ID            { get; set; }
    [JsonProperty("title")]           public string Title         { get; set; } = null!;
    [JsonProperty("author")]          public string Author        { get; set; } = null!;
    [JsonProperty("home_image_path")] public string HomeImagePath { get; set; } = null!;
    [JsonProperty("publish_date")]    public string PublishDate   { get; set; } = null!;
    [JsonProperty("summary")]         public string Summary       { get; set; } = null!;
    [JsonProperty("sort_index")]      public int    SortIndex     { get; set; }
}
