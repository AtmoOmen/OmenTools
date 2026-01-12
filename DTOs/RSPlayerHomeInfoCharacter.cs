using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerHomeInfoCharacter
{
    [JsonProperty("create_time")]      public string? CreateTime     { get; set; }
    [JsonProperty("gender")]           public string? Gender         { get; set; }
    [JsonProperty("last_login_time")]  public string? LastLoginTime  { get; set; }
    [JsonProperty("race")]             public string? Race           { get; set; }
    [JsonProperty("character_name")]   public string? CharacterName  { get; set; }
    [JsonProperty("area_id")]          public int     AreaID         { get; set; }
    [JsonProperty("play_time")]        public string? PlayTime       { get; set; }
    [JsonProperty("house_info")]       public string? HouseInfo      { get; set; }
    [JsonProperty("house_remain_day")] public string? HouseRemainDay { get; set; }
    [JsonProperty("group_id")]         public int     GroupID        { get; set; }
    [JsonProperty("guild_name")]       public string? GuildName      { get; set; }
    [JsonProperty("fc_id")]            public string? FcID           { get; set; }
    [JsonProperty("tribe")]            public string? Tribe          { get; set; }
    [JsonProperty("guild_tag")]        public string? GuildTag       { get; set; }
    [JsonProperty("washing_num")]      public int     WashingNum     { get; set; }
    [JsonProperty("treasure_times")]   public string? TreasureTimes  { get; set; }
    [JsonProperty("kill_times")]       public string? KillTimes      { get; set; }
    [JsonProperty("newrank")]          public string? Newrank        { get; set; }
    [JsonProperty("crystal_rank")]     public string? CrystalRank    { get; set; }
    [JsonProperty("fish_times")]       public string? FishTimes      { get; set; }
}
