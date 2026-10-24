using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace OmenTools.Info.Game;

public readonly record struct EorzeaDate
(
    long EorzeaTimeStamp
)
{
    public int Year => checked((int)FloorDivide(EorzeaTimeStamp, EORZEA_SECONDS_PER_YEAR));

    public int Month => (int)(GetCycleSeconds(EORZEA_SECONDS_PER_YEAR) / EORZEA_SECONDS_PER_MONTH) + 1;

    public int Day => (int)(GetCycleSeconds(EORZEA_SECONDS_PER_MONTH) / EORZEA_SECONDS_PER_DAY) + 1;

    public int Hour => SecondsOfDay / 3600;

    public int Minute => SecondsOfDay / 60 % 60;

    public int Second => SecondsOfDay % 60;

    public int SecondsOfDay => (int)GetCycleSeconds(EORZEA_SECONDS_PER_DAY);

    public TimeSpan TimeOfDay => TimeSpan.FromSeconds(SecondsOfDay);

    public override string ToString()
        => $"{Year}/{Month}/{Day} {Hour:D2}:{Minute:D2}:{Second:D2}";

    public string ToShortString() =>
        $"{Hour:D2}:{Minute:D2}:{Second:D2}";

    public static EorzeaDate GetTime
    (
        long? timeStamp = null
    )
        => new(FloorDivide(checked(((timeStamp ?? Framework.GetServerTime()) - UNIX_EPOCH_OFFSET) * EORZEA_TIME_NUMERATOR), EORZEA_TIME_DENOMINATOR));

    public long ToUnixTimeSeconds()
        => UNIX_EPOCH_OFFSET - FloorDivide(checked(-EorzeaTimeStamp * EORZEA_TIME_DENOMINATOR), EORZEA_TIME_NUMERATOR);

    private long GetCycleSeconds
    (
        long cycle
    )
        => EorzeaTimeStamp - (FloorDivide(EorzeaTimeStamp, cycle) * cycle);

    private static long FloorDivide
    (
        long dividend,
        long divisor
    )
    {
        var quotient = Math.DivRem(dividend, divisor, out var remainder);
        return remainder < 0 ?
                   quotient - 1 :
                   quotient;
    }

    #region 常量

    public const long EORZEA_SECONDS_PER_DAY          = 86400;
    public const long REAL_SECONDS_PER_EORZEA_HOUR    = 175;
    public const long REAL_SECONDS_PER_EORZEA_DAY     = 24 * REAL_SECONDS_PER_EORZEA_HOUR;
    public const long REAL_SECONDS_PER_WEATHER_PERIOD = 8  * REAL_SECONDS_PER_EORZEA_HOUR;

    private const long UNIX_EPOCH_OFFSET        = 1278950400;
    private const long EORZEA_TIME_NUMERATOR    = 144;
    private const long EORZEA_TIME_DENOMINATOR  = 7;
    private const long EORZEA_SECONDS_PER_MONTH = 32 * EORZEA_SECONDS_PER_DAY;
    private const long EORZEA_SECONDS_PER_YEAR  = 12 * EORZEA_SECONDS_PER_MONTH;

    #endregion
}
