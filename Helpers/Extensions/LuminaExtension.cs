using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Excel.Sheets;

namespace OmenTools.Helpers;

public static class LuminaExtension
{
    public static string GetTexturePath(this Map map) 
    {
        var mapKey = map.Id.ExtractText();
        var rawKey = mapKey.Replace("/", "");
        return $"ui/map/{mapKey}/{rawKey}_m.tex";
    }

    public static Vector2 GetPositionWorld(this Aetheryte aetheryte)
    {
        var mapRow = aetheryte.Territory.ValueNullable?.Map.ValueNullable;
        if (mapRow == null) return new();

        return MapToWorld(GetPositionMap(aetheryte), (Map)mapRow);
    }

    public static Vector2 GetPositionMap(this Aetheryte aetheryte)
    {
        var mapRow = aetheryte.Territory.ValueNullable?.Map.ValueNullable;
        if (mapRow == null) return new();

        var mapRowNotNull = (Map)mapRow;

        var result = LuminaGetter.GetSub<MapMarker>()
                                 .SelectMany(x => x)
                                 .Where(x => x.DataType == 3 && x.RowId == mapRowNotNull.MapMarkerRange && x.DataKey.RowId == aetheryte.RowId)
                                 .Select(x => TextureToMap(x.X, x.Y, mapRowNotNull.SizeFactor))
                                 .FirstOrDefault();

        return result;
    }

    public static List<MapMarker> GetMapMarkers(this Map map) =>
        LuminaGetter.GetSub<MapMarker>()
                    .SelectMany(x => x)
                    .Where(x => x.RowId == map.MapMarkerRange)
                    .ToList();

    private static string GetMarkerPlaceName(this MapMarker marker)
    {
        var placeName = marker.GetMarkerLabel();
        if (placeName != string.Empty) return placeName;

        if (!LuminaGetter.TryGetRow<MapSymbol>(marker.Icon, out var symbol)) return string.Empty;
        return symbol.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty;
    }

    public static string GetMarkerLabel(this MapMarker marker) => 
        marker.PlaceNameSubtext.ValueNullable?.Name.ExtractText() ?? string.Empty;

    public static Vector2 GetPosition(this MapMarker marker) => new(marker.X, marker.Y);

    public static BitmapFontIcon ToBitmapFontIcon(this ClassJob? job)
    {
        if (job == null || job.Value.RowId == 0) return BitmapFontIcon.NewAdventurer;

        return job.Value.RowId switch
        {
            < 1      => BitmapFontIcon.NewAdventurer,
            < 41     => (BitmapFontIcon)job.Value.RowId + 127,
            41 or 42 => (BitmapFontIcon)job.Value.RowId + 129,
            _        => BitmapFontIcon.NewAdventurer
        };
    }

    public static BitmapFontIcon ToBitmapFontIcon(this ClassJob job)
    {
        if (job.RowId == 0) return BitmapFontIcon.NewAdventurer;

        return job.RowId switch
        {
            < 1      => BitmapFontIcon.NewAdventurer,
            < 41     => (BitmapFontIcon)job.RowId + 127,
            41 or 42 => (BitmapFontIcon)job.RowId + 129,
            _        => BitmapFontIcon.NewAdventurer
        };
    }

    public static string ExtractPlaceName(this TerritoryType row) => 
        row.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty;

    public static Vector3 ToPosition(this Level level) => new(level.X, level.Y, level.Z);
}
