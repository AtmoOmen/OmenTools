using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Data;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;
using OmenTools.OmenService.Abstractions;
using OmenTools.Threading.TaskHelper;
using AccountInfo = FFXIVClientStructs.Expansion.FFXIV.Client.Game.AccountInfo;
using AgentUpdateDelegate = FFXIVClientStructs.Expansion.FFXIV.Client.UI.Agent.AgentUpdateDelegate;
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

    private Hook<AgentUpdateDelegate>? AgentMapUpdateHook;

    private ulong       lastAccountID;
    private TaskHelper? switchClassJobTaskHelper;

    protected override unsafe void Init()
    {
        switchClassJobTaskHelper ??= new()
        {
            TimeoutMS       = 3_000,
            RetryIntervalMS = 50
        };

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
        switchClassJobTaskHelper?.Dispose();
        switchClassJobTaskHelper = null;

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
    ///     当前玩家的位置
    /// </summary>
    /// <remarks>始终有效，若本地玩家对象为空则取摄像机位置。</remarks>
    public static unsafe Vector3 Position =>
        Framework.Instance()->BGCollisionModule->ForcedStreamingSphere.ToVector3();

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
    public static unsafe bool IsInParty =>
        GameState.PartyMemberCount > 1;

    /// <summary>
    ///     当前是否在本地队伍中
    /// </summary>
    public static unsafe bool IsInLocalParty
    {
        get
        {
            if (InfoProxyCrossRealm.IsCrossRealmParty() ||
                InfoProxyCrossRealm.IsAllianceRaid())
                return false;

            return GroupManager.Instance()->GetGroupWithCheck()->MemberCount > 1;
        }
    }

    /// <summary>
    ///     当前是否在本地队伍中
    /// </summary>
    public static unsafe bool IsInCrossRealmParty =>
        InfoProxyCrossRealm.IsCrossRealmParty();

    /// <summary>
    ///     当前是否为队长
    /// </summary>
    public static bool IsPartyLeader =>
        InfoProxyCrossRealm.IsLocalPlayerPartyLeader();

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
    ///     获取当前玩家指定职业是否已解锁。
    /// </summary>
    public static unsafe bool IsClassJobUnlocked
    (
        uint classJobID
    ) =>
        GetClassJobLevel(classJobID) != 0;

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

    /// <summary>
    ///     切换至指定职业，优先切换已保存套装，否则从兵装库穿戴最高装等主手并按需处理灵魂水晶
    /// </summary>
    public static unsafe void SwitchClassJob
    (
        uint classJob
    )
    {
        if (classJob == 0        ||
            ClassJob == classJob ||
            Object   == null)
            return;

        if (!LuminaGetter.TryGetRow(classJob, out ClassJob jobData) ||
            jobData.ExpArrayIndex < 0)
            return;

        var playerState = PlayerState.Instance();
        if (playerState == null)
            return;

        var jobLevel = playerState->GetClassJobLevel((int)classJob, false);
        if (jobLevel == 0)
            return;

        Instance().switchClassJobTaskHelper?.Abort();

        if (RaptureGearsetModule.Instance() != null             &&
            TryFindClassJobGearset(classJob, out var gearsetID) &&
            SwitchGearset(gearsetID))
            return;

        var inventoryManager = InventoryManager.Instance();
        if (inventoryManager == null)
            return;

        var pvpProfile = PvPProfile.Instance();
        var pvpRank = pvpProfile != null ?
                          pvpProfile->GetPvPRank() :
                          (byte)0;

        var parentClassJobID  = jobData.ClassJobParent.RowId;
        var soulCrystalItemID = jobData.ItemSoulCrystal.RowId;
        var requiresSoulCrystal = parentClassJobID  != classJob &&
                                  soulCrystalItemID != 0;

        ushort soulCrystalSlot = 0;
        var    hasEquippedSoul = false;

        if (requiresSoulCrystal)
        {
            var equippedSoul = inventoryManager->GetInventorySlot(InventoryType.EquippedItems, SOUL_CRYSTAL_SLOT);
            hasEquippedSoul = equippedSoul != null     &&
                              !equippedSoul->IsEmpty() &&
                              equippedSoul->GetBaseItemId() == soulCrystalItemID;

            if (!hasEquippedSoul &&
                !TryFindItemSlot(inventoryManager, InventoryType.ArmorySoulCrystal, soulCrystalItemID, out soulCrystalSlot))
                return;
        }

        var equippedMainHand = inventoryManager->GetInventorySlot(InventoryType.EquippedItems, MAIN_HAND_SLOT);

        if (equippedMainHand != null     &&
            !equippedMainHand->IsEmpty() &&
            TryCanEquipMainHand(equippedMainHand, playerState, jobLevel, (byte)classJob, pvpRank, out _))
        {
            if (requiresSoulCrystal)
            {
                if (!hasEquippedSoul)
                    inventoryManager->MoveItemSlot(InventoryType.ArmorySoulCrystal, soulCrystalSlot, InventoryType.EquippedItems, SOUL_CRYSTAL_SLOT, true);

                return;
            }

            var equippedSoul = inventoryManager->GetInventorySlot(InventoryType.EquippedItems, SOUL_CRYSTAL_SLOT);

            if (equippedSoul != null &&
                !equippedSoul->IsEmpty())
            {
                var soulContainer = inventoryManager->GetInventoryContainer(InventoryType.ArmorySoulCrystal);

                if (soulContainer != null &&
                    soulContainer->IsLoaded)
                {
                    for (var i = 0; i < soulContainer->Size; i++)
                    {
                        var slot = soulContainer->GetInventorySlot(i);
                        if (slot == null ||
                            !slot->IsEmpty())
                            continue;

                        inventoryManager->MoveItemSlot(InventoryType.EquippedItems, SOUL_CRYSTAL_SLOT, InventoryType.ArmorySoulCrystal, (ushort)i);
                        return;
                    }
                }

                return;
            }
        }

        var mainHandContainer = inventoryManager->GetInventoryContainer(InventoryType.ArmoryMainHand);
        if (mainHandContainer == null ||
            !mainHandContainer->IsLoaded)
            return;

        var    foundMainHand = false;
        ushort bestSlot      = 0;
        uint   bestItemLevel = 0;
        var    bestIsHQ      = false;

        for (var i = 0; i < mainHandContainer->Size; i++)
        {
            var slot = mainHandContainer->GetInventorySlot(i);
            if (slot == null    ||
                slot->IsEmpty() ||
                !TryCanEquipMainHand(slot, playerState, jobLevel, (byte)classJob, pvpRank, out var itemData))
                continue;

            var itemLevel     = itemData.LevelItem.RowId;
            var isHighQuality = slot->IsHighQuality();
            var isBetter = !foundMainHand            ||
                           itemLevel > bestItemLevel ||
                           (itemLevel == bestItemLevel &&
                            isHighQuality              &&
                            !bestIsHQ);
            if (!isBetter)
                continue;

            foundMainHand = true;
            bestSlot      = (ushort)i;
            bestItemLevel = itemLevel;
            bestIsHQ      = isHighQuality;
        }

        if (!foundMainHand)
            return;

        inventoryManager->MoveItemSlot(InventoryType.ArmoryMainHand, bestSlot, InventoryType.EquippedItems, MAIN_HAND_SLOT, true);

        if (!requiresSoulCrystal)
            return;

        Instance().switchClassJobTaskHelper?.Enqueue
        (
            () => TryEquipPendingSoulCrystal(classJob, parentClassJobID, soulCrystalItemID),
            $"SwitchClassJob_{classJob}"
        );
    }

    private static unsafe bool TryEquipPendingSoulCrystal
    (
        uint targetClassJobID,
        uint parentClassJobID,
        uint soulCrystalItemID
    )
    {
        if (Object   == null ||
            ClassJob == targetClassJobID)
            return true;

        if (ClassJob != parentClassJobID)
            return false;

        var manager = InventoryManager.Instance();
        if (manager == null)
            return true;

        if (TryFindItemSlot(manager, InventoryType.ArmorySoulCrystal, soulCrystalItemID, out var targetSoulSlot))
            manager->MoveItemSlot(InventoryType.ArmorySoulCrystal, targetSoulSlot, InventoryType.EquippedItems, SOUL_CRYSTAL_SLOT, true);

        return true;
    }

    private static unsafe bool TryCanEquipMainHand
    (
        InventoryItem* item,
        PlayerState*   playerState,
        ushort         jobLevel,
        byte           classJobID,
        byte           pvpRank,
        out Item       itemData
    )
    {
        itemData = default;

        if (item->IsCollectable())
            return false;

        var baseItemID = item->GetBaseItemId();
        if (baseItemID == 0 ||
            !LuminaGetter.TryGetRow(baseItemID, out itemData))
            return false;

        if (itemData.EquipSlotCategory is not { IsValid: true, Value.MainHand: 1 })
            return false;

        return InventoryManager.CanEquip
               (
                   baseItemID,
                   playerState->Race,
                   playerState->Sex,
                   jobLevel,
                   classJobID,
                   playerState->GrandCompany,
                   pvpRank,
                   null
               ) ==
               0;
    }

    private static unsafe bool TryFindItemSlot
    (
        InventoryManager* inventoryManager,
        InventoryType     inventoryType,
        uint              baseItemID,
        out ushort        slotIndex
    )
    {
        slotIndex = 0;

        var container = inventoryManager->GetInventoryContainer(inventoryType);
        if (container == null ||
            !container->IsLoaded)
            return false;

        for (var i = 0; i < container->Size; i++)
        {
            var slot = container->GetInventorySlot(i);
            if (slot == null    ||
                slot->IsEmpty() ||
                slot->GetBaseItemId() != baseItemID)
                continue;

            slotIndex = (ushort)i;
            return true;
        }

        return false;
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
    ) => Object == null ?
             float.MaxValue :
             Vector3.Distance(Object.Position, target);

    public static float DistanceTo3DSquared
    (
        Vector3 target
    ) => Object == null ?
             float.MaxValue :
             Vector3.DistanceSquared(Object.Position, target);

    public static float DistanceTo2D
    (
        Vector2 target
    ) => Object == null ?
             float.MaxValue :
             Vector2.Distance(Object.Position.ToVector2(), target);

    public static float DistanceTo2DSquared
    (
        Vector2 target
    ) => Object == null ?
             float.MaxValue :
             Vector2.DistanceSquared(Object.Position.ToVector2(), target);

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

    #region 常量

    private const ushort MAIN_HAND_SLOT    = 0;
    private const ushort SOUL_CRYSTAL_SLOT = 13;

    #endregion
}
