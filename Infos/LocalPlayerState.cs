using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;

namespace OmenTools.Infos;

public unsafe class LocalPlayerState
{
    private delegate ushort GetClassJobLevelDelegate(PlayerState* instance, uint classJobID, bool checkParentJob);
    private static readonly GetClassJobLevelDelegate GetClassJobLevelInternal =
        new CompSig("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? 4C 8D 3D").GetDelegate<GetClassJobLevelDelegate>();

    private delegate nint GetAccountInfoInstanceDelegate();
    private static readonly GetAccountInfoInstanceDelegate GetAccountInfoInstance = 
        new CompSig("48 8B 05 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 83 39").GetDelegate<GetAccountInfoInstanceDelegate>();
    
    /// <summary>
    /// 当前玩家是否正在移动
    /// </summary>
    public static bool IsMoving => 
        AgentMap.Instance()->IsPlayerMoving;
    
    /// <summary>
    /// 当前玩家的 EntityID
    /// </summary>
    public static uint EntityID => 
        PlayerState.Instance()->EntityId;

    /// <summary>
    /// 当前玩家的 AccountID
    /// </summary>
    public static ulong AccountID =>
        *(ulong*)(GetAccountInfoInstance() + 8);
    
    /// <summary>
    /// 当前 ClassJob 表数据
    /// </summary>
    public static ClassJob ClassJobData =>
        LuminaGetter.GetRow<ClassJob>(ClassJob).GetValueOrDefault();

    /// <summary>
    /// 当前职业
    /// </summary>
    public static uint ClassJob => 
        AgentHUD.Instance()->CharacterClassJobId;

    /// <summary>
    /// 当前玩家的 ContentID
    /// </summary>
    public static ulong ContentID => 
        PlayerState.Instance()->ContentId;

    /// <summary>
    /// 当前玩家的等级
    /// </summary>
    public static ushort CurrentLevel =>
        (ushort)PlayerState.Instance()->CurrentLevel;

    /// <summary>
    /// 当前玩家该职业下最高可以达到的等级
    /// </summary>
    public static ushort MaxLevel =>
        PlayerState.Instance()->MaxLevel;

    /// <summary>
    /// 当前玩家获得的最优队员推荐次数
    /// </summary>
    public static ushort Commendations =>
        (ushort)PlayerState.Instance()->PlayerCommendations;

    /// <summary>
    /// 当前是否为等级同步状态
    /// </summary>
    public static bool IsLevelSynced =>
        PlayerState.Instance()->IsLevelSynced != 0;

    /// <summary>
    /// 当前玩家对象
    /// </summary>
    public static IPlayerCharacter? Object =>
        DService.ObjectTable.LocalPlayer;

    /// <summary>
    /// 获取当前玩家指定职业的等级
    /// </summary>
    public static ushort GetClassJobLevel(uint classJobID, bool checkParentJob = true) =>
        ClassJob == classJobID ? CurrentLevel : GetClassJobLevelInternal(PlayerState.Instance(), classJobID, checkParentJob);

    /// <summary>
    /// 获取当前玩家第一个可用的职业套装
    /// </summary>
    public static bool TryFindClassJobGearset(uint classJobID, out byte gearsetID)
    {
        gearsetID = 0;
        
        var gearsetModule = RaptureGearsetModule.Instance();
        for (var i = 0; i < 100; i++)
        {
            var gearset = gearsetModule->GetGearset(i);
            if (gearset == null                                                          ||
                !gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists)         ||
                gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.MainHandMissing) ||
                gearset->Id       != i                                                   ||
                gearset->ClassJob != classJobID)
                continue;

            gearsetID = gearset->Id;
            return true;
        }
            
        return false;
    }

    public static bool SwitchGearset(uint classJob)
    {
        if (Object == null || !TryFindClassJobGearset(classJob, out var gearsetID)) return false;
        
        ChatHelper.SendMessage($"/gearset change {gearsetID + 1}");
        return true;
    }
    
    public static bool SwitchGearset(byte gearsetID)
    {
        if (Object == null) return false;
        
        var gearset = RaptureGearsetModule.Instance()->GetGearset(gearsetID);
        if (gearset == null                                                  ||
            !gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists) ||
            gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.MainHandMissing))
            return false;
        
        ChatHelper.SendMessage($"/gearset change {gearsetID + 1}");
        return true;
    }

    public static bool HasStatus(uint statusID, out int index, uint sourceID = 0xE0000000)
    {
        index = -1;
        if (Object == null) return false;

        index = Object.ToStruct()->StatusManager.GetStatusIndex(statusID, sourceID);
        return index != -1;
    }

    public static uint GetItemCount(uint itemID)
    {
        var instance = InventoryManager.Instance();
        return (uint)(instance->GetInventoryItemCount(itemID) + instance->GetInventoryItemCount(itemID, true));
    }

    public static float DistanceTo3D(Vector3 distance)
    {
        if (Object == null) return float.MaxValue;
        return Vector3.Distance(Object.Position, distance);
    }
    
    public static float DistanceTo2D(Vector2 distance)
    {
        if (Object == null) return float.MaxValue;
        return Vector2.Distance(Object.Position.ToVector2(), distance);
    }
}
