using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerHomeInfoCareer
{
    [JsonProperty("career")]          public string? Career         { get; set; }
    [JsonProperty("character_level")] public string? CharacterLevel { get; set; }
    [JsonProperty("part_date")]       public string? PartDate       { get; set; }
    [JsonProperty("update_date")]     public string? UpdateDate     { get; set; }
    [JsonProperty("career_type")]     public string? CareerType     { get; set; }
}
