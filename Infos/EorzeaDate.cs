using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace OmenTools.Infos;

public class EorzeaDate
(
    int  y,
    int  m,
    int  d,
    int  h,
    int  min,
    int  sec,
    long timestamp
)
{
    public int  Year            { get; set; } = y;
    public int  Month           { get; set; } = m;
    public int  Day             { get; set; } = d;
    public int  Hour            { get; set; } = h;
    public int  Minute          { get; set; } = min;
    public int  Second          { get; set; } = sec;
    public long EorzeaTimeStamp { get; set; } = timestamp;

    public override string ToString()
        => $"{Year}/{Month}/{Day} {(Hour < 10 ? "0" + Hour : Hour)}:{(Minute < 10 ? "0" + Minute : Minute)}:{(Second < 10 ? "0" + Second : Second)}";

    public string ToShortString() =>
        $"{(Hour < 10 ? "0" + Hour : Hour)}:{(Minute < 10 ? "0" + Minute : Minute)}:{(Second < 10 ? "0" + Second : Second)}";

    public static EorzeaDate GetTime(long? timeStamp = null)
    {
        // 时间偏移量
        const long TIME_ADJUST = 1278950400;
        // 游戏时间
        const long TIME_GAME = 144;
        // 一周天数
        const long TIME_EARTH = 7;
        
        var timeEorzea =
            Convert.ToInt64
            (
                Math.Round
                (
                    Convert.ToDecimal
                    (
                        ((timeStamp ?? Framework.GetServerTime()) - TIME_ADJUST) *
                        TIME_GAME /
                        TIME_EARTH
                    ),
                    0
                )
            );
        
        timeEorzea = Convert.ToInt64(Math.Round((double)(timeEorzea / 10), 0)) * 10;
        var etY = Convert.ToInt32(Math.Floor((decimal)(timeEorzea / 33177600)));
        var etM = Convert.ToInt32(Math.Floor((decimal)(timeEorzea % 33177600 / 2764800))) + 1;
        var etD = Convert.ToInt32(Math.Floor((decimal)(timeEorzea % 2764800) / 86400))    + 1;
        var etH = Convert.ToInt32(Math.Floor((decimal)(timeEorzea % 86400) / 3600));
        var etm = Convert.ToInt32(Math.Floor((decimal)(timeEorzea % 3600)  / 60));
        var ets = Convert.ToInt32(timeEorzea % 60);
        return new EorzeaDate(etY, etM, etD, etH, etm, ets, timeEorzea);
    }

    public static unsafe uint GetWeather(uint? zoneID = null)
        => zoneID == null
               ? WeatherManager.Instance()->GetCurrentWeather()
               : WeatherManager.Instance()->GetWeatherForDaytime((ushort)zoneID, GetTime().Hour);
}
