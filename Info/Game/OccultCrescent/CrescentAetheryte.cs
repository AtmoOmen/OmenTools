using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Info.Game;

/// <summary>
///     新月岛中的 魔路 / 简易魔路 数据类
/// </summary>
public class CrescentAetheryte
(
    uint                   dataID,
    Vector3                position,
    CrescentAetherytePoint point
) : IEquatable<CrescentAetheryte>
{
    /// <summary>
    ///     调查队营地
    /// </summary>
    public static CrescentAetheryte ExpeditionBaseCamp { get; } = new(4927, new(834.0f, 73.0f, -694.6f), CrescentAetherytePoint.ExpeditionBaseCamp);

    /// <summary>
    ///     放浪神圣域遗迹
    /// </summary>
    public static CrescentAetheryte WanderersHaven { get; } = new(4928, new(-169, 6.5f, -609.1f), CrescentAetherytePoint.WanderersHaven);

    /// <summary>
    ///     水晶洞窟
    /// </summary>
    public static CrescentAetheryte CrystallizedCaverns { get; } = new(4929, new(-354.4f, 100f, -120.2f), CrescentAetherytePoint.CrystallizedCaverns);

    /// <summary>
    ///     古树湿原
    /// </summary>
    public static CrescentAetheryte Eldergrowth { get; } = new(4930, new(302.9f, 103f, 305.4f), CrescentAetherytePoint.Eldergrowth);

    /// <summary>
    ///     石塔水沼
    /// </summary>
    public static CrescentAetheryte Stonemarsh { get; } = new(4947, new(-384.9f, 97.4f, 277.1f), CrescentAetherytePoint.Stonemarsh);

    /// <summary>
    ///     北部调查队营地
    /// </summary>
    public static CrescentAetheryte NorthHornBaseCamp { get; } = new(5571, new(881.1f, 258.5f, 882.2f), CrescentAetherytePoint.NorthHornBaseCamp);

    /// <summary>
    ///     卡纳克城塞
    /// </summary>
    public static CrescentAetheryte CrownOfKarnak { get; } = new(5576, new(454.0f, 70.0f, 530.6f), CrescentAetherytePoint.CrownOfKarnak);

    /// <summary>
    ///     沉没圣堂前
    /// </summary>
    public static CrescentAetheryte SinkingSanctuary { get; } = new(5572, new(358.2f, 45.1f, -557.3f), CrescentAetherytePoint.SinkingSanctuary);

    /// <summary>
    ///     浮游遗迹
    /// </summary>
    public static CrescentAetheryte SuspendedMasonry { get; } = new(5573, new(-549.2f, 67.2f, 597.2f), CrescentAetherytePoint.SuspendedMasonry);

    /// <summary>
    ///     腐坏的街道前
    /// </summary>
    public static CrescentAetheryte MolderingOutskirts { get; } = new(5574, new(-386.6f, 39.2f, -437.6f), CrescentAetherytePoint.MolderingOutskirts);

    /// <summary>
    ///     妖火渔村
    /// </summary>
    public static CrescentAetheryte UnhallowedHamlet { get; } = new(5575, new(-15.7f, 2.1f, -44.5f), CrescentAetherytePoint.UnhallowedHamlet);

    /// <summary>
    ///     南征之章中所有 魔路 / 简易魔路 数据
    /// </summary>
    public static List<CrescentAetheryte> SouthHornAetherytes { get; } =
    [
        ExpeditionBaseCamp, WanderersHaven, CrystallizedCaverns, Eldergrowth, Stonemarsh
    ];

    /// <summary>
    ///     北征之章中所有 魔路 / 简易魔路 数据
    /// </summary>
    public static List<CrescentAetheryte> NorthHornAetherytes { get; } =
    [
        NorthHornBaseCamp, CrownOfKarnak, SinkingSanctuary, SuspendedMasonry, MolderingOutskirts, UnhallowedHamlet
    ];

    /// <summary>
    ///     获取南征之章中离当前最近且可用的魔路
    /// </summary>
    /// <param name="destination">目的地位置</param>
    /// <param name="result">目的地魔路</param>
    /// <returns>是否成功获取可用的魔路</returns>
    public static bool TryGetNearestSouthHorn(Vector3 destination, [NotNullWhen(true)] out CrescentAetheryte? result)
    {
        result = null;

        var minTime = float.MaxValue;

        foreach (var aetheryte in SouthHornAetherytes)
        {
            if (IsNeedToUseAetheryte(destination, aetheryte.Position, out var time) && time < minTime)
            {
                result  = aetheryte;
                minTime = time;
            }
        }

        return minTime != float.MaxValue;
    }

    private static bool IsNeedToUseAetheryte(Vector3 finalPos, Vector3 aetherytePos, out float aetheryteTime)
    {
        aetheryteTime = float.MaxValue;

        if (DService.Instance().ObjectTable.LocalPlayer is not { } localPlayer)
            return false;

        if (Vector3.DistanceSquared(localPlayer.Position, finalPos) <= 1_0000f)
            return false;

        var directTime = Vector3.Distance(finalPos, localPlayer.Position) / 12f;
        aetheryteTime = (Vector3.Distance(finalPos, aetherytePos)         / 12f) + 10f;
        return directTime > aetheryteTime;
    }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; } = LuminaWrapper.GetPlaceName(dataID);

    /// <summary>
    ///     PlaceName ID
    /// </summary>
    public uint DataID { get; } = dataID;

    /// <summary>
    ///     传送后位置 (为传送到目标魔路后玩家所在位置, 非魔路对象位置)
    /// </summary>
    public Vector3 Position { get; } = position;

    /// <summary>
    ///     类型
    /// </summary>
    public CrescentAetherytePoint Point { get; } = point;

    /// <summary>
    ///     传送界面显示索引
    /// </summary>
    public byte Index { get; } = (byte)((byte)point > 4 ? (byte)point - 4 : (byte)point);

    /// <summary>
    ///     使用魔路传送至目标魔路, 需要当前传送界面已打开
    /// </summary>
    /// <returns>false - 传送界面未打开; true - 传送请求发送成功</returns>
    public unsafe bool TeleportTo()
    {
        var agent = AgentTelepotTown.Instance();
        if (agent == null || !agent->IsAgentActive()) return false;

        agent->TeleportToAetheryte(Index);
        return true;
    }

    public bool Equals(CrescentAetheryte? other) =>
        DataID == other?.DataID;

    public override bool Equals(object? obj) =>
        obj is CrescentAetheryte other && Equals(other);

    public override int GetHashCode() =>
        (int)DataID;

    public static bool operator ==(CrescentAetheryte left, CrescentAetheryte right) =>
        left.Equals(right);

    public static bool operator !=(CrescentAetheryte left, CrescentAetheryte right) =>
        !left.Equals(right);
}

public enum CrescentAetherytePoint : byte
{
    /// <summary>
    ///     调查队营地
    /// </summary>
    ExpeditionBaseCamp = 0,

    /// <summary>
    ///     放浪神圣域遗迹
    /// </summary>
    WanderersHaven = 1,

    /// <summary>
    ///     水晶洞窟
    /// </summary>
    CrystallizedCaverns = 2,

    /// <summary>
    ///     古树湿原
    /// </summary>
    Eldergrowth = 3,

    /// <summary>
    ///     石塔水沼
    /// </summary>
    Stonemarsh = 4,

    /// <summary>
    ///     北部调查队营地
    /// </summary>
    NorthHornBaseCamp = 5,

    /// <summary>
    ///     卡纳克城塞
    /// </summary>
    CrownOfKarnak = 6,

    /// <summary>
    ///     沉没圣堂前
    /// </summary>
    SinkingSanctuary = 7,

    /// <summary>
    ///     浮游遗迹
    /// </summary>
    SuspendedMasonry = 8,

    /// <summary>
    ///     腐坏的街道前
    /// </summary>
    MolderingOutskirts = 9,

    /// <summary>
    ///     妖火渔村
    /// </summary>
    UnhallowedHamlet = 10
}
