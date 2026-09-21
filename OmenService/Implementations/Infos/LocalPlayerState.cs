using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Data;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;
using OmenTools.OmenService.Abstractions;
using AccountInfo = OmenTools.Interop.Game.Models.Native.AccountInfo;
using AgentUpdateDelegate = OmenTools.Interop.Game.Models.Native.AgentUpdateDelegate;
using Control = FFXIVClientStructs.FFXIV.Client.Game.Control.Control;
using CurrencyManager = FFXIVClientStructs.FFXIV.Client.Game.CurrencyManager;
using GrandCompany = FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany;
using Task = System.Threading.Tasks.Task;

namespace OmenTools.OmenService;

public class LocalPlayerState : OmenServiceBase<LocalPlayerState>
{
    private static readonly CompSig AccountInfoUpdateSig =
        new("40 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 8B 01");
    private unsafe delegate bool AccountInfoUpdateDelegate
    (
        AccountInfo* accountInfo,
        ulong        accountID,
        byte         forceReload
    );
    private Hook<AccountInfoUpdateDelegate>? AccountInfoUpdateHook;

    private static readonly CompSig AccountInfoCacheLoadSig =
        new("4C 8B DC 55 41 56 49 8D 6B ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 ?? 48 8B 05");
    private unsafe delegate nint AccountInfoCacheLoadDelegate
    (
        ulong accountID,
        nint  arg2,
        nint  data,
        nuint dataSize,
        byte  arg5,
        byte  arg6
    );
    private Hook<AccountInfoCacheLoadDelegate>? AccountInfoCacheLoadHook;

    private delegate bool IsLocalPlayerInPartyDelegate();
    private static readonly IsLocalPlayerInPartyDelegate IsLocalPlayerInParty =
        new CompSig("E8 ?? ?? ?? ?? 84 C0 74 25 48 8B 8D ?? ?? ?? ??").GetDelegate<IsLocalPlayerInPartyDelegate>();

    private delegate bool IsLocalPlayerPartyLeaderDelegate();
    private static readonly IsLocalPlayerPartyLeaderDelegate IsLocalPlayerPartyLeader =
        new CompSig("48 83 EC ?? E8 ?? ?? ?? ?? 84 C0 74 ?? 48 83 C4").GetDelegate<IsLocalPlayerPartyLeaderDelegate>();

    private Hook<AgentUpdateDelegate>? AgentMapUpdateHook;

    private ulong lastAccountID;

    protected override unsafe void Init()
    {
        AgentMapUpdateHook ??= IGameInteropProvider.Instance().HookFromAddress<AgentUpdateDelegate>
        (
            AgentMap.Instance()->VirtualTable->GetVFuncByName("Update"),
            AgentMapUpdateDetour
        );
        AgentMapUpdateHook.Enable();

        AccountInfoUpdateHook ??= AccountInfoUpdateSig.GetHook<AccountInfoUpdateDelegate>(AccountInfoUpdateDetour);
        AccountInfoUpdateHook.Enable();

        AccountInfoCacheLoadHook ??= AccountInfoCacheLoadSig.GetHook<AccountInfoCacheLoadDelegate>(AccountInfoCacheLoadDetour);
        AccountInfoCacheLoadHook.Enable();
    }

    protected override void Uninit()
    {
        AgentMapUpdateHook?.Dispose();
        AgentMapUpdateHook = null;

        AccountInfoUpdateHook?.Dispose();
        AccountInfoUpdateHook = null;

        AccountInfoCacheLoadHook?.Dispose();
        AccountInfoCacheLoadHook = null;
    }

    private unsafe void AgentMapUpdateDetour
    (
        AgentInterface* agent,
        uint            frameCount
    )
    {
        AgentMapUpdateHook.Original(agent, frameCount);

        var isMovingNow = ((AgentMap*)agent)->IsPlayerMoving;
        if (IsMoving != isMovingNow)
            PlayerMoveStateChanged?.Invoke(isMovingNow);
        IsMoving = isMovingNow;
    }

    private unsafe bool AccountInfoUpdateDetour
    (
        AccountInfo* accountInfo,
        ulong        accountID,
        byte         forceReload
    )
    {
        var result = AccountInfoUpdateHook.Original(accountInfo, accountID, forceReload);
        NotifyAccountIDChanged();
        return result;
    }

    private unsafe nint AccountInfoCacheLoadDetour
    (
        ulong accountID,
        nint  arg2,
        nint  data,
        nuint dataSize,
        byte  arg5,
        byte  arg6
    )
    {
        var result = AccountInfoCacheLoadHook.Original(accountID, arg2, data, dataSize, arg5, arg6);
        NotifyAccountIDChanged();
        return result;
    }

    private unsafe void NotifyAccountIDChanged()
    {
        var accountInfo = AccountInfo.Instance();
        if (accountInfo == null) return;

        var accountID = accountInfo->AccountID;
        if (Interlocked.Exchange(ref lastAccountID, accountID) == accountID) return;

        AccountIDChanged?.Invoke(accountID);
    }

    /// <summary>
    ///     玩家移动状态变更时
    /// </summary>
    public event Action<bool>? PlayerMoveStateChanged;

    /// <summary>
    ///     当前玩家的 AccountID 变更时
    /// </summary>
    public event Action<ulong>? AccountIDChanged;

    /// <summary>
    ///     当前玩家所属的大国防联军
    /// </summary>
    public static unsafe GrandCompany GrandCompany =>
        (GrandCompany)PlayerState.Instance()->GrandCompany;

    /// <summary>
    ///     当前玩家是否正在移动
    /// </summary>
    public bool IsMoving { get; private set; }

    /// <summary>
    ///     当前玩家是否在小队中
    /// </summary>
    public static bool IsInAnyParty =>
        InfoProxyCrossRealm.IsCrossRealmParty() || IPartyList.Instance().Length >= 2;

    /// <summary>
    ///     当前玩家是否正处于步行模式
    /// </summary>
    public static unsafe bool IsWalking =>
        Control.Instance()->IsWalking;

    /// <summary>
    ///     当前玩家的性别
    /// </summary>
    public static unsafe byte Sex =>
        PlayerState.Instance()->Sex;

    /// <summary>
    ///     当前玩家的用户名
    /// </summary>
    public static unsafe string Name =>
        PlayerState.Instance()->CharacterNameString;

    /// <summary>
    ///     当前玩家的 EntityID
    /// </summary>
    public static unsafe uint EntityID =>
        PlayerState.Instance()->EntityId;

    /// <summary>
    ///     当前玩家的 AccountID
    /// </summary>
    public static unsafe ulong AccountID =>
        AccountInfo.Instance()->AccountID;

    /// <summary>
    ///     当前 ClassJob 表数据
    /// </summary>
    public static ClassJob ClassJobData =>
        LuminaGetter.GetRow<ClassJob>(ClassJob).GetValueOrDefault();

    /// <summary>
    ///     当前职业
    /// </summary>
    public static unsafe uint ClassJob =>
        AgentHUD.Instance()->CharacterClassJobId;

    /// <summary>
    ///     当前玩家的 ContentID
    /// </summary>
    public static unsafe ulong ContentID =>
        PlayerState.Instance()->ContentId;

    /// <summary>
    ///     当前玩家的等级
    /// </summary>
    public static unsafe ushort CurrentLevel =>
        (ushort)PlayerState.Instance()->CurrentLevel;

    /// <summary>
    ///     当前玩家该职业下最高可以达到的等级
    /// </summary>
    public static unsafe ushort MaxLevel =>
        PlayerState.Instance()->MaxLevel;

    /// <summary>
    ///     当前玩家获得的最优队员推荐次数
    /// </summary>
    public static unsafe ushort Commendations =>
        (ushort)PlayerState.Instance()->PlayerCommendations;

    /// <summary>
    ///     当前是否为等级同步状态
    /// </summary>
    public static unsafe bool IsLevelSynced =>
        PlayerState.Instance()->IsLevelSynced;

    /// <summary>
    ///     当前是否在任一队伍中
    /// </summary>
    public static bool IsInParty =>
        IsLocalPlayerInParty();

    /// <summary>
    ///     当前是否为队长
    /// </summary>
    public static bool IsPartyLeader =>
        IsLocalPlayerPartyLeader();

    /// <summary>
    ///     当前玩家对象
    /// </summary>
    public static IPlayerCharacter? Object =>
        IObjectTable.Instance().LocalPlayer;

    /// <summary>
    ///     获取当前玩家指定职业的等级
    /// </summary>
    public static unsafe ushort GetClassJobLevel
    (
        uint classJobID,
        bool shouldGetSynced = true
    ) =>
        ClassJob == classJobID ?
            CurrentLevel :
            PlayerState.Instance()->GetClassJobLevel((int)classJobID, shouldGetSynced);

    /// <summary>
    ///     获取当前玩家第一个可用的职业套装
    /// </summary>
    public static unsafe bool TryFindClassJobGearset
    (
        uint     classJobID,
        out byte gearsetID
    )
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

    /// <summary>
    ///     获取当前玩家第一个可用的职业套装
    /// </summary>
    public static unsafe bool TryFindClassJobGearsetData
    (
        uint                                  classJobID,
        out RaptureGearsetModule.GearsetEntry gearsetData
    )
    {
        gearsetData = default;

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

            gearsetData = *gearset;
            return true;
        }

        return false;
    }

    private static bool IsOnClassJobChanging;

    public static unsafe bool SwitchGearset
    (
        uint classJob
    )
    {
        if (!LuminaGetter.TryGetRow<ClassJob>(classJob, out var jobData)) return false;
        if (Object == null) return false;

        if (TryFindClassJobGearset(classJob, out var gearsetID))
        {
            ChatManager.Instance().SendMessage($"/gearset change {gearsetID + 1}");
            return true;
        }

        if (Inventories.PlayerWithArmory.TryGetFirstItem
            (
                x => LuminaGetter.TryGetRow(x.GetBaseItemId(), out Item mainHandItemData) &&
                     mainHandItemData.ClassJobCategory.Value.IsClassJobIn(classJob)       &&
                     mainHandItemData.LevelEquip <= GetClassJobLevel(classJob)            &&
                     mainHandItemData.EquipSlotCategory is { IsValid: true, Value.MainHand: 1 },
                out var mainHandItem
            ))
        {
            InventoryManager.Instance()->MoveItemSlot(mainHandItem->GetInventoryType(), mainHandItem->GetSlot(), InventoryType.EquippedItems, 0, true);

            if (jobData.DohDolJobIndex > -1 &&
                Inventories.PlayerWithArmory.TryGetFirstItem
                (
                    x => LuminaGetter.TryGetRow(x.GetBaseItemId(), out Item offHandItemData) &&
                         offHandItemData.ClassJobCategory.Value.IsClassJobIn(classJob)       &&
                         offHandItemData.LevelEquip <= GetClassJobLevel(classJob)            &&
                         offHandItemData.EquipSlotCategory is { IsValid: true, Value.OffHand: 1 },
                    out var offHandItem
                ))
            {
                Task.Run
                (() =>
                    {
                        if (IsOnClassJobChanging) return;
                        IsOnClassJobChanging = true;

                        try
                        {
                            var timeout = Environment.TickCount64 + 2_000;

                            while (ClassJob != classJob || Environment.TickCount64 > timeout)
                                Thread.Sleep(100);

                            if (offHandItem == null) return;
                            InventoryManager.Instance()->MoveItemSlot(offHandItem->GetInventoryType(), offHandItem->GetSlot(), InventoryType.EquippedItems, 1);
                        }
                        finally
                        {
                            IsOnClassJobChanging = false;
                        }
                    }
                );
            }

            return true;
        }

        return false;
    }

    public static unsafe bool SwitchGearset
    (
        byte gearsetID
    )
    {
        if (Object == null) return false;

        var gearset = RaptureGearsetModule.Instance()->GetGearset(gearsetID);
        if (gearset == null                                                  ||
            !gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists) ||
            gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.MainHandMissing))
            return false;

        ChatManager.Instance().SendMessage($"/gearset change {gearsetID + 1}");
        return true;
    }

    public static unsafe bool HasStatus
    (
        uint    statusID,
        out int index,
        uint    sourceID = 0xE0000000
    )
    {
        index = -1;
        if (Object == null) return false;

        index = Object.ToStruct()->StatusManager.GetStatusIndex(statusID, sourceID);
        return index != -1;
    }

    public static unsafe uint GetItemCount
    (
        uint itemID
    )
    {
        var manager = CurrencyManager.Instance();
        if (manager->HasItem(itemID))
            return manager->GetItemCount(itemID);

        var instance = InventoryManager.Instance();
        return (uint)(instance->GetInventoryItemCount(itemID) + instance->GetInventoryItemCount(itemID, true));
    }

    public static float DistanceTo3D
    (
        Vector3 target
    )
    {
        if (Object == null)
            return float.MaxValue;

        return Vector3.Distance(Object.Position, target);
    }

    public static float DistanceTo3DSquared
    (
        Vector3 target
    )
    {
        if (Object == null)
            return float.MaxValue;

        return Vector3.DistanceSquared(Object.Position, target);
    }

    public static float DistanceTo2D
    (
        Vector2 target
    )
    {
        if (Object == null)
            return float.MaxValue;

        return Vector2.Distance(Object.Position.ToVector2(), target);
    }

    public static float DistanceTo2DSquared
    (
        Vector2 target
    )
    {
        if (Object == null)
            return float.MaxValue;

        return Vector2.DistanceSquared(Object.Position.ToVector2(), target);
    }

    public static float DistanceToObject2D
    (
        IGameObject? target,
        bool         ignoreRadius = true
    )
    {
        if (target == null || Object == null)
            return float.MaxValue;

        // 考虑在里面
        if (!ignoreRadius)
        {
            if (DistanceTo2D(target.Position.ToVector2()) <= target.HitboxRadius)
                return 0f;

            if (!(GetNearestPointToObject(target) is var nearestPoint) || nearestPoint == Vector3.Zero)
                return 0f;

            return DistanceTo2D(nearestPoint.ToVector2());
        }

        return DistanceTo2D(target.Position.ToVector2());
    }

    public static float DistanceToObject2DSquared
    (
        IGameObject? target,
        bool         ignoreRadius = true
    )
    {
        if (target == null || Object == null)
            return float.MaxValue;

        if (!ignoreRadius)
        {
            if (DistanceTo2DSquared(target.Position.ToVector2()) <= target.HitboxRadius * target.HitboxRadius)
                return 0f;

            if (!(GetNearestPointToObject(target) is var nearestPoint) || nearestPoint == Vector3.Zero)
                return 0f;

            return DistanceTo2DSquared(nearestPoint.ToVector2());
        }

        return DistanceTo2DSquared(target.Position.ToVector2());
    }

    public static float DistanceToObject3D
    (
        IGameObject? target,
        bool         ignoreRadius = true
    )
    {
        if (target == null || Object == null)
            return float.MaxValue;

        // 考虑在里面
        if (!ignoreRadius)
        {
            if (DistanceTo2D(target.Position.ToVector2()) <= target.HitboxRadius)
                return 0f;

            if (!(GetNearestPointToObject(target) is var nearestPoint) || nearestPoint == Vector3.Zero)
                return 0f;

            return DistanceTo3D(nearestPoint);
        }

        return DistanceTo3D(target.Position);
    }

    public static float DistanceToObject3DSquared
    (
        IGameObject? target,
        bool         ignoreRadius = true
    )
    {
        if (target == null || Object == null)
            return float.MaxValue;

        if (!ignoreRadius)
        {
            if (DistanceTo2DSquared(target.Position.ToVector2()) <= target.HitboxRadius * target.HitboxRadius)
                return 0f;

            if (!(GetNearestPointToObject(target) is var nearestPoint) || nearestPoint == Vector3.Zero)
                return 0f;

            return DistanceTo3DSquared(nearestPoint);
        }

        return DistanceTo3DSquared(target.Position);
    }

    public static Vector3 GetNearestPointToObject
    (
        IGameObject? target
    )
    {
        if (target == null || Object == null)
            return Vector3.One;

        // 现在就在里面
        if (DistanceTo2D(target.Position.ToVector2()) <= target.HitboxRadius)
            return Object.Position;

        return target.Position + (Vector3.Normalize(Object.Position - target.Position) * target.HitboxRadius);
    }
}
