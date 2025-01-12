using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static Vector2 TextureToMap(int x, int y, float scale)
    {
        var num = scale / 100f;
        return new Vector2(
            ConvertRawPositionToMapCoordinate((int)((x - 1024) * num * 1000f), scale),
            ConvertRawPositionToMapCoordinate((int)((y - 1024) * num * 1000f), scale));
    }

    private static float ConvertRawPositionToMapCoordinate(int pos, float scale)
    {
        var num1 = scale / 100f;
        var num2 = (float)(pos * (double)num1 / 1000.0f);
        return (40.96f / num1 * ((num2 + 1024.0f) / 2048.0f)) + 1.0f;
    }

    public static Vector2 WorldToMap(Vector2 worldCoordinates, Map map) =>
        new(
            WorldXZToMap(worldCoordinates.X, map.SizeFactor, map.OffsetX),
            WorldXZToMap(worldCoordinates.Y, map.SizeFactor, map.OffsetY));

    public static Vector3 WorldToMap(Vector3 worldCoordinates, Map map, TerritoryTypeTransient territoryTransient, bool correctZOffset = false) =>
        new(
            WorldXZToMap(worldCoordinates.X, map.SizeFactor, map.OffsetX),
            WorldXZToMap(worldCoordinates.Z, map.SizeFactor, map.OffsetY),
            WorldYToMap(worldCoordinates.Y, territoryTransient.OffsetZ, correctZOffset));

    public static Vector2 MapToWorld(Vector2 mapCoordinates, Map map) =>
        new(
            MapToWorldXZ(mapCoordinates.X, map.SizeFactor, map.OffsetX),
            MapToWorldXZ(mapCoordinates.Y, map.SizeFactor, map.OffsetY));

    public static Vector3 MapToWorld(Vector3 mapCoordinates, Map map, TerritoryTypeTransient territoryTransient, bool correctZOffset = false) =>
        new(
            MapToWorldXZ(mapCoordinates.X, map.SizeFactor, map.OffsetX),
            MapToWorldXZ(mapCoordinates.Z, map.SizeFactor, map.OffsetY),
            MapToWorldY(mapCoordinates.Y, territoryTransient.OffsetZ, correctZOffset));

    public static Vector2 WorldToTexture(Vector3 position, Map map) =>
        new Vector2(position.X, position.Z) * (map.SizeFactor / 100.0f) +
        new Vector2(map.OffsetX, map.OffsetY) * (map.SizeFactor / 100.0f) +
        new Vector2(1024.0f, 1024.0f);

    public static Vector2 TextureToWorld(Vector2 coordinates, Map map)
    {
        var adjustedCoordinates = (coordinates - new Vector2(1024f)) / map.SizeFactor;
        return adjustedCoordinates * 100f - new Vector2(map.OffsetX, map.OffsetY);
    }

    private static float WorldXZToMap(float value, uint scale, int offset) 
        => 0.02f * offset + 2048f / scale + 0.02f * value + 1f;

    private static float MapToWorldXZ(float mapValue, uint scale, int offset) 
        => (mapValue - 0.02f * offset - 2048f / scale - 1f) / 0.02f;

    public static float WorldYToMap(float value, int zOffset, bool correctZOffset = false) 
        => (correctZOffset && zOffset == -10000) ? value / 100f : (value - zOffset) / 100f;

    public static float MapToWorldY(float mapValue, int zOffset, bool correctZOffset = false) 
        => (correctZOffset && zOffset == -10000) ? mapValue * 100f : mapValue * 100f + zOffset;

    public static List<MapMarker> GetZoneMapMarkers(uint zoneID) =>
        LuminaCache.Get<Map>()!
            .Where(x => x.TerritoryType.RowId == zoneID)
            .SelectMany(x => x.GetMapMarkers())
            .ToList();

    public static List<MapMarker> GetMapMarkers(uint mapID)
        => LuminaCache.GetRow<Map>(mapID)?.GetMapMarkers() ?? [];
}
