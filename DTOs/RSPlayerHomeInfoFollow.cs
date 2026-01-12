using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerHomeInfoFollow
{
    [JsonProperty("followNum")] public int FollowNum { get; set; }
    [JsonProperty("fansNum")]   public int FansNum   { get; set; }
}
