using System.Numerics;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Info.Game.ItemSource.Models;

public class ShopNPCLocation
(
    float x,
    float y,
    uint  territoryID,
    uint? map = null
)
{
    public float X { get; } = x;

    public float Y { get; } = y;

    public uint TerritoryID { get; } = territoryID;

    public uint MapID { get; } = map ?? LuminaGetter.GetRowOrDefault<TerritoryType>(territoryID).Map.RowId;

    public Vector2 TexturePosition =>
        new(X, Y);

    public Vector2 MapPosition
    {
        get
        {
            var mapInfo = LuminaGetter.GetRowOrDefault<Map>(MapID);
            return ToMapPos(new Vector2(X, Y), mapInfo.SizeFactor, new Vector2(mapInfo.OffsetX, mapInfo.OffsetY));
        }
    }

    public TerritoryType GetTerritory() =>
        LuminaGetter.GetRowOrDefault<TerritoryType>(TerritoryID);

    public Map GetMap() =>
        LuminaGetter.GetRowOrDefault<Map>(MapID);

    private static Vector2 ToMapPos(Vector2 pos, float scale, Vector2 offset)
    {
        var x = ToMapPos(pos.X, scale, (short)offset.X);
        var y = ToMapPos(pos.Y, scale, (short)offset.Y);
        return new(x, y);
    }

    private static float ToMapPos(float val, float scale, short offset)
    {
        var c = scale / 100.0f;

        val = (val + offset) * c;

        return 41.0f / c * ((val + 1024.0f) / 2048.0f) + 1;
    }
}
