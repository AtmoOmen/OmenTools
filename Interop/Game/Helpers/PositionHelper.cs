using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Interop.Game.Helpers;

// FF14 坐标体系
//
// 纹理坐标 (Texture): 二维, 地图贴图上的像素位置, 范围约 0~2048, 中心 1024
// 地图坐标 (Map):     二维, 游戏内地图界面显示的坐标, 范围约 1~42
// 世界坐标 (World):   三维, XZ 为水平面, Y 为纵轴高度
//
// 三套坐标的水平换算统一以 "World <-> Texture" 为精确基准, 其余全部由它线性派生, 保证任意链路自洽:
//
//   s       = SizeFactor / 100
//   Texture = (World_xz + Offset) * s + 1024
//   Map     = Texture / 2048 * (PIXELS_PER_MAP_UNIT / s) + 1
//
// 高度轴 (World.Y) 与水平面无关, 单独换算
public static class PositionHelper
{
    // 单位地图坐标对应的纹理像素数, 即 2048 / 50
    private const float PIXELS_PER_MAP_UNIT = 40.96f;

    private const float TEXTURE_CENTER = 1024f;
    private const float TEXTURE_SIZE   = 2048f;

    // 高度轴的哨兵偏移值, 表示该地图未定义独立的高度偏移
    private const int HEIGHT_OFFSET_SENTINEL = -10000;

    #region World <-> Texture

    public static Vector2 WorldToTexture(Vector3 pos, Map map) =>
        WorldXZToTexture(new(pos.X, pos.Z), map.SizeFactor, map.OffsetX, map.OffsetY);

    public static Vector2 TextureToWorld(Vector2 pos, Map map) =>
        TextureToWorldXZ(pos, map.SizeFactor, map.OffsetX, map.OffsetY);

    #endregion

    #region World <-> Map

    public static Vector2 WorldToMap(Vector2 pos, Map map) =>
        TextureToMapXZ(WorldXZToTexture(pos, map.SizeFactor, map.OffsetX, map.OffsetY), map.SizeFactor);

    public static Vector3 WorldToMap
    (
        Vector3                pos,
        Map                    map,
        TerritoryTypeTransient territoryTransient,
        bool                   correctHeightOffset = false
    )
    {
        var mapXZ = TextureToMapXZ(WorldXZToTexture(new(pos.X, pos.Z), map.SizeFactor, map.OffsetX, map.OffsetY), map.SizeFactor);
        return new(mapXZ.X, WorldYToMap(pos.Y, territoryTransient.OffsetZ, correctHeightOffset), mapXZ.Y);
    }

    public static Vector2 MapToWorld(Vector2 pos, Map map) =>
        TextureToWorldXZ(MapToTextureXZ(pos, map.SizeFactor), map.SizeFactor, map.OffsetX, map.OffsetY);

    public static Vector3 MapToWorld
    (
        Vector3                pos,
        Map                    map,
        TerritoryTypeTransient territoryTransient,
        bool                   correctHeightOffset = false
    )
    {
        var worldXZ = TextureToWorldXZ(MapToTextureXZ(new(pos.X, pos.Z), map.SizeFactor), map.SizeFactor, map.OffsetX, map.OffsetY);
        return new(worldXZ.X, MapToWorldY(pos.Y, territoryTransient.OffsetZ, correctHeightOffset), worldXZ.Y);
    }

    #endregion

    #region Texture <-> Map

    public static Vector2 TextureToMap(int x, int y, float sizeFactor) =>
        TextureToMapXZ(new(x, y), (uint)sizeFactor);

    public static Vector2 TextureToMap(Vector2 pos, Map map) =>
        TextureToMapXZ(pos, map.SizeFactor);

    public static Vector2 MapToTexture(Vector2 pos, Map map) =>
        MapToTextureXZ(pos, map.SizeFactor);

    #endregion

    #region Height (World.Y <-> Map)

    public static float WorldYToMap(float value, int heightOffset, bool correctHeightOffset = false) =>
        correctHeightOffset && heightOffset == HEIGHT_OFFSET_SENTINEL ? value / 100f : (value - heightOffset) / 100f;

    public static float MapToWorldY(float mapValue, int heightOffset, bool correctHeightOffset = false) =>
        correctHeightOffset && heightOffset == HEIGHT_OFFSET_SENTINEL ? mapValue * 100f : (mapValue * 100f) + heightOffset;

    #endregion

    #region Core

    private static Vector2 WorldXZToTexture(Vector2 worldXZ, uint sizeFactor, int offsetX, int offsetY)
    {
        var s = sizeFactor / 100f;
        return ((worldXZ + new Vector2(offsetX, offsetY)) * s) + new Vector2(TEXTURE_CENTER);
    }

    private static Vector2 TextureToWorldXZ(Vector2 texture, uint sizeFactor, int offsetX, int offsetY)
    {
        var s = sizeFactor / 100f;
        return ((texture - new Vector2(TEXTURE_CENTER)) / s) - new Vector2(offsetX, offsetY);
    }

    private static Vector2 TextureToMapXZ(Vector2 texture, uint sizeFactor)
    {
        var s = sizeFactor / 100f;
        return (texture    / TEXTURE_SIZE * (PIXELS_PER_MAP_UNIT / s)) + new Vector2(1f);
    }

    private static Vector2 MapToTextureXZ(Vector2 map, uint sizeFactor)
    {
        var s = sizeFactor / 100f;
        return (map - new Vector2(1f)) * s / PIXELS_PER_MAP_UNIT * TEXTURE_SIZE;
    }

    #endregion
}
