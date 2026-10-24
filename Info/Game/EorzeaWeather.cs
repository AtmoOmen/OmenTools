using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Info.Game;

public static class EorzeaWeather
{
    public static unsafe WeatherRate GetWeatherRate
    (
        uint territoryTypeID
    )
    {
        var territory = LuminaGetter.GetRowOrDefault<TerritoryType>(territoryTypeID);

        if (territory.ExcelPage.ReadPackedBool(territory.RowOffset + 59, 1))
        {
            foreach (var festival in GameMain.Instance()->ActiveFestivals)
            {
                if (festival.Id == 0) continue;

                var festivalData  = LuminaGetter.GetRowOrDefault<Festival>(festival.Id);
                var weatherRateID = festivalData.ExcelPage.ReadUInt8(festivalData.RowOffset + 5);
                if (weatherRateID != 0)
                    return LuminaGetter.GetRowOrDefault<WeatherRate>(weatherRateID);
            }
        }

        return territory.WeatherRate.Value;
    }

    public static uint GetWeather
    (
        WeatherRate weatherRate,
        long        serverTime
    )
    {
        var weatherCount = 0;
        var totalWeight  = 0U;

        while (weatherCount < weatherRate.Weather.Count && weatherRate.Weather[weatherCount].RowId != 0)
        {
            totalWeight += weatherRate.Rate[weatherCount];
            weatherCount++;
        }

        if (totalWeight == 0)
            throw new InvalidOperationException($"WeatherRate {weatherRate.RowId} has no weather rates.");

        var weatherPeriod    = serverTime                             / EorzeaDate.REAL_SECONDS_PER_WEATHER_PERIOD;
        var days             = weatherPeriod                          / 3;
        var nextPeriodHour   = (weatherPeriod                + 1) % 3 * 8;
        var seed             = unchecked((uint)((days * 100) + nextPeriodHour));
        var mixed            = (seed  << 11) ^ seed;
        var hash             = (mixed >> 8)  ^ mixed;
        var target           = unchecked(hash + (hash / uint.MaxValue)) % totalWeight;
        var cumulativeWeight = 0U;

        for (var i = 0; i < weatherCount; i++)
        {
            cumulativeWeight += weatherRate.Rate[i];
            if (target < cumulativeWeight)
                return weatherRate.Weather[i].RowId;
        }

        throw new InvalidOperationException($"WeatherRate {weatherRate.RowId} has invalid weather rates.");
    }
}
