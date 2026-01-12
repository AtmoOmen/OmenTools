using Newtonsoft.Json;

namespace OmenTools.DTOs;

public record RSActivityCalendarData
{
    [JsonProperty("id")]         public uint    ID        { get; set; }
    [JsonProperty("name")]       public string  Name      { get; set; } = null!;
    [JsonProperty("url")]        public string  URL       { get; set; } = null!;
    [JsonProperty("begin_time")] public int     BeginTime { get; set; }
    [JsonProperty("end_time")]   public int     EndTime   { get; set; }
    [JsonProperty("weight")]     public uint    Weight    { get; set; }
    [JsonProperty("color")]      public string  Color     { get; set; } = null!;
    [JsonProperty("type")]       public int     Type      { get; set; }
    [JsonProperty("daoyu_sw")]   public int     DaoyuSw   { get; set; }
    [JsonProperty("banner_url")] public string? BannerURL { get; set; }
}
