using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSPlayerHomeInfoData
{
    [JsonProperty("id")]                      public int                               ID                   { get; set; }
    [JsonProperty("uuid")]                    public string?                           UUID                 { get; set; }
    [JsonProperty("character_name")]          public string?                           CharacterName        { get; set; }
    [JsonProperty("area_id")]                 public int                               AreaID               { get; set; }
    [JsonProperty("area_name")]               public string?                           AreaName             { get; set; }
    [JsonProperty("group_id")]                public int                               GroupID              { get; set; }
    [JsonProperty("group_name")]              public string?                           GroupName            { get; set; }
    [JsonProperty("avatar")]                  public string?                           Avatar               { get; set; }
    [JsonProperty("profile")]                 public string?                           Profile              { get; set; }
    [JsonProperty("weekday_time")]            public string?                           WeekdayTime          { get; set; }
    [JsonProperty("weekend_time")]            public string?                           WeekendTime          { get; set; }
    [JsonProperty("qq")]                      public string                            Qq                   { get; set; } = null!;
    [JsonProperty("career_publish")]          public int                               CareerPublish        { get; set; }
    [JsonProperty("guild_publish")]           public int                               GuildPublish         { get; set; }
    [JsonProperty("create_time_publish")]     public int                               CreateTimePublish    { get; set; }
    [JsonProperty("last_login_time_publish")] public int                               LastLoginTimePublish { get; set; }
    [JsonProperty("play_time_publish")]       public int                               PlayTimePublish      { get; set; }
    [JsonProperty("house_info_publish")]      public int                               HouseInfoPublish     { get; set; }
    [JsonProperty("washing_num_publish")]     public int                               WashingNumPublish    { get; set; }
    [JsonProperty("achieve_publish")]         public int                               AchievePublish       { get; set; }
    [JsonProperty("resently_publish")]        public int                               ResentlyPublish      { get; set; }
    [JsonProperty("experience")]              public string?                           Experience           { get; set; }
    [JsonProperty("theme_id")]                public string?                           ThemeID              { get; set; }
    [JsonProperty("test_limited_badge")]      public int                               TestLimitedBadge     { get; set; }
    [JsonProperty("posts2_creator_badge")]    public int                               Posts2CreatorBadge   { get; set; }
    [JsonProperty("admin_tag")]               public int                               AdminTag             { get; set; }
    [JsonProperty("publish_tab")]             public string?                           PublishTab           { get; set; }
    [JsonProperty("achieve_tab")]             public string?                           AchieveTab           { get; set; }
    [JsonProperty("treasure_times_publish")]  public int                               TreasureTimesPublish { get; set; }
    [JsonProperty("kill_times_publish")]      public int                               KillTimesPublish     { get; set; }
    [JsonProperty("newrank_publish")]         public int                               NewrankPublish       { get; set; }
    [JsonProperty("crystal_rank_publish")]    public int                               CrystalRankPublish   { get; set; }
    [JsonProperty("fish_times_publish")]      public int                               FishTimesPublish     { get; set; }
    [JsonProperty("collapse_badge")]          public int                               CollapseBadge        { get; set; }
    [JsonProperty("achieveInfo")]             public List<RSPlayerHomeInfoAchievement> AchieveInfo          { get; set; } = [];
    [JsonProperty("careerLevel")]             public List<RSPlayerHomeInfoCareer>      CareerLevel          { get; set; } = [];
    [JsonProperty("characterDetail")]         public List<RSPlayerHomeInfoCharacter>   CharacterDetail      { get; set; } = [];
    [JsonProperty("followFansiNum")]          public RSPlayerHomeInfoFollow?           FollowFansiNum       { get; set; }
    [JsonProperty("interactNum")]             public int                               InteractNum          { get; set; }
    [JsonProperty("beLikedNum")]              public string?                           BeLikedNum           { get; set; }
    [JsonProperty("relation")]                public int                               Relation             { get; set; }
}
