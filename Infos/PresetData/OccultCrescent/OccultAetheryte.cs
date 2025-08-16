using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Infos;

/// <summary>
/// 新月岛中的 魔路 / 简易魔路 数据类
/// </summary>
/// <param name="dataID">PlaceName ID</param>
/// <param name="position">位置 (为传送到目标魔路后玩家所在位置, 非魔路对象位置)</param>
/// <param name="point">类型</param>
public class OccultAetheryte(uint dataID, Vector3 position, OccultAetherytePoint point) : IEquatable<OccultAetheryte>
{
    /// <summary>
    /// 调查队营地
    /// </summary>
    public static OccultAetheryte ExpeditionBaseCamp { get; } = new(4927, new(834.0f, 73.0f, -694.6f), OccultAetherytePoint.ExpeditionBaseCamp);

    /// <summary>
    /// 放浪神圣域遗迹
    /// </summary>
    public static OccultAetheryte WanderersHaven { get; } = new(4928, new(-169, 6.5f, -609.1f), OccultAetherytePoint.WanderersHaven);

    /// <summary>
    /// 水晶洞窟
    /// </summary>
    public static OccultAetheryte CrystallizedCaverns { get; } = new(4929, new(-354.4f, 100f, -120.2f), OccultAetherytePoint.CrystallizedCaverns);

    /// <summary>
    /// 古树湿原
    /// </summary>
    public static OccultAetheryte Eldergrowth { get; } = new(4930, new(302.9f, 103f, 305.4f), OccultAetherytePoint.Eldergrowth);

    /// <summary>
    /// 石塔水沼
    /// </summary>
    public static OccultAetheryte Stonemarsh { get; } = new(4947, new(-384.9f, 97.4f, 277.1f), OccultAetherytePoint.Stonemarsh);

    /// <summary>
    /// 南征之章中所有 魔路 / 简易魔路 数据
    /// </summary>
    public static List<OccultAetheryte> SouthHornAetherytes { get; } =
    [
        ExpeditionBaseCamp, WanderersHaven, CrystallizedCaverns, Eldergrowth, Stonemarsh
    ];
    
    /// <summary>
    /// 获取南征之章中离当前最近且可用的魔路
    /// </summary>
    /// <param name="destination">目的地位置</param>
    /// <param name="result">目的地魔路</param>
    /// <returns>是否成功获取可用的魔路</returns>
    public static bool TryGetNearestSouthHorn(Vector3 destination, [NotNullWhen(true)] out OccultAetheryte? result)
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
            
        if (DService.ObjectTable.LocalPlayer is not { } localPlayer)
            return false;

        if (Vector3.DistanceSquared(localPlayer.Position, finalPos) <= 1_0000f)
            return false;

        var directTime    = Vector3.Distance(finalPos, localPlayer.Position) / 12f;
        aetheryteTime = (Vector3.Distance(finalPos,    aetherytePos)         / 12f) + 10f;
        return directTime > aetheryteTime;
    }
    
    public string               Name     { get; } = LuminaWrapper.GetPlaceName(dataID);
    public uint                 DataID   { get; } = dataID;
    public Vector3              Position { get; } = position;
    public OccultAetherytePoint Point    { get; } = point;
    public byte                 Index    { get; } = (byte)((byte)point > 4 ? (byte)point - 4 : (byte)point);

    /// <summary>
    /// 使用魔路传送至目标魔路, 需要当前传送界面已打开
    /// </summary>
    /// <returns>false - 传送界面未打开; true - 传送请求发送成功</returns>
    public unsafe bool TeleportTo()
    {
        var agent = AgentTelepotTown.Instance();
        if (agent == null || !agent->IsAgentActive()) return false;
        
        agent->TeleportToAetheryte(Index);
        return true;
    }
    
    public bool Equals(OccultAetheryte? other) => 
        DataID == other.DataID;

    public override bool Equals(object? obj) => 
        obj is OccultAetheryte other && Equals(other);

    public override int GetHashCode() => 
        (int)DataID;

    public static bool operator ==(OccultAetheryte left, OccultAetheryte right) => 
        left.Equals(right);

    public static bool operator !=(OccultAetheryte left, OccultAetheryte right) => 
        !left.Equals(right);
}

public enum OccultAetherytePoint : byte
{
    /// <summary>
    /// 调查队营地
    /// </summary>
    ExpeditionBaseCamp = 0,
    
    /// <summary>
    /// 放浪神圣域遗迹
    /// </summary>
    WanderersHaven = 1,
    
    /// <summary>
    /// 水晶洞窟
    /// </summary>
    CrystallizedCaverns = 2,
    
    /// <summary>
    /// 古树湿原
    /// </summary>
    Eldergrowth = 3,
    
    /// <summary>
    /// 石塔水沼
    /// </summary>
    Stonemarsh = 4,
}
