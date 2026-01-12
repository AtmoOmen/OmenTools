using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerAchievementSearchData
{
    [JsonProperty("event_type")]    public string   EventType   { get; set; } = string.Empty;
    [JsonProperty("detail")]        public string   Detail      { get; set; } = string.Empty;
    [JsonProperty("event_type_id")] public uint     EventTypeID { get; set; }
    [JsonProperty("log_time")]      public DateTime LogTime     { get; set; }
    [JsonProperty("part_date")]     public DateTime PartDate    { get; set; }
}
