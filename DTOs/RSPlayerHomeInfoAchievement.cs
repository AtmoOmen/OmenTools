using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerHomeInfoAchievement
{
    [JsonProperty("medal_id")]       public string? MedalID       { get; set; }
    [JsonProperty("medal_type")]     public string? MedalType     { get; set; }
    [JsonProperty("achieve_id")]     public string? AchieveID     { get; set; }
    [JsonProperty("achieve_time")]   public string? AchieveTime   { get; set; }
    [JsonProperty("group_id")]       public string? GroupID       { get; set; }
    [JsonProperty("character_name")] public string? CharacterName { get; set; }
    [JsonProperty("medal_type_id")]  public string? MedalTypeID   { get; set; }
    [JsonProperty("achieve_name")]   public string? AchieveName   { get; set; }
    [JsonProperty("area_id")]        public string? AreaID        { get; set; }
    [JsonProperty("achieve_detail")] public string? AchieveDetail { get; set; }
    [JsonProperty("part_date")]      public string? PartDate      { get; set; }
}
