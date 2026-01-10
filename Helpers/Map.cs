using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static Vector2 TextureToMap(int x, int y, float scale)
    {
        var num = scale / 100f;
        return new(ConvertRawPosToMapPos((int)((x - 1024) * num * 1000f), scale),
                   ConvertRawPosToMapPos((int)((y - 1024) * num * 1000f), scale));
    }
    
    public static Vector2 WorldToMap(Vector2 pos, Map map) =>
        new(WorldXZToMap(pos.X, map.SizeFactor, map.OffsetX),
            WorldXZToMap(pos.Y, map.SizeFactor, map.OffsetY));

    public static Vector3 WorldToMap(
        Vector3                pos,
        Map                    map,
        TerritoryTypeTransient territoryTransient,
        bool                   correctZOffset = false) =>
        new(WorldXZToMap(pos.X, map.SizeFactor, map.OffsetX),
            WorldXZToMap(pos.Z, map.SizeFactor, map.OffsetY),
            WorldYToMap(pos.Y, territoryTransient.OffsetZ, correctZOffset));

    public static Vector2 MapToWorld(Vector2 pos, Map map) =>
        new(MapToWorldXZ(pos.X, map.SizeFactor, map.OffsetX),
            MapToWorldXZ(pos.Y, map.SizeFactor, map.OffsetY));

    public static Vector3 MapToWorld(
        Vector3                pos,
        Map                    map,
        TerritoryTypeTransient territoryTransient,
        bool                   correctZOffset = false) =>
        new(MapToWorldXZ(pos.X, map.SizeFactor, map.OffsetX),
            MapToWorldXZ(pos.Z, map.SizeFactor, map.OffsetY),
            MapToWorldY(pos.Y, territoryTransient.OffsetZ, correctZOffset));

    public static Vector2 WorldToTexture(Vector3 pos, Map map) =>
        (new Vector2(pos.X,       pos.Z)       * (map.SizeFactor / 100.0f)) +
        (new Vector2(map.OffsetX, map.OffsetY) * (map.SizeFactor / 100.0f)) +
        new Vector2(1024.0f, 1024.0f);

    public static Vector2 TextureToWorld(Vector2 pos, Map map)
    {
        var adjustedCoordinates = (pos - new Vector2(1024f)) / map.SizeFactor;
        return (adjustedCoordinates * 100f) - new Vector2(map.OffsetX, map.OffsetY);
    }

    private static float WorldXZToMap(float value, uint scale, int offset) => 
        (0.02f * offset) + (2048f / scale) + (0.02f * value) + 1f;

    private static float MapToWorldXZ(float mapValue, uint scale, int offset) => 
        (mapValue - (0.02f * offset) - (2048f / scale) - 1f) / 0.02f;

    public static float WorldYToMap(float value, int zOffset, bool correctZOffset = false) =>
        correctZOffset && zOffset == -10000 ? value / 100f : (value - zOffset) / 100f;

    public static float MapToWorldY(float mapValue, int zOffset, bool correctZOffset = false) => 
        correctZOffset && zOffset == -10000 ? mapValue * 100f : (mapValue * 100f) + zOffset;

    public static List<MapMarker> GetZoneMapMarkers(uint zoneID) =>
        LuminaGetter.Get<Map>()
                    .Where(x => x.TerritoryType.RowId == zoneID)
                    .SelectMany(x => x.GetMapMarkers())
                    .ToList();

    public static List<MapMarker> GetMapMarkers(uint mapID) => 
        LuminaGetter.TryGetRow<Map>(mapID, out var row) ? row.GetMapMarkers() : [];
    
    private static float ConvertRawPosToMapPos(int pos, float scale)
    {
        var num1 = scale / 100f;
        var num2 = (float)(pos * (double)num1 / 1000.0f);
        return (40.96f / num1 * ((num2 + 1024.0f) / 2048.0f)) + 1.0f;
    }
}
