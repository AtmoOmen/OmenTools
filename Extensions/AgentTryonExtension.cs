using System.Runtime.CompilerServices;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;
using AgentTryonExpansion = FFXIVClientStructs.Expansion.FFXIV.Client.UI.Agent.AgentTryon;

namespace OmenTools.Extensions;

/// <summary>
///     <see cref="AgentTryon"/> 与 <see cref="AgentTryonExpansion"/> 的试穿扩展方法。
/// </summary>
public static unsafe class AgentTryonExtension
{
    extension
    (
        scoped ref AgentTryon agent
    )
    {
        /// <summary>
        ///     在试穿室中批量试穿装备与眼镜。
        /// </summary>
        /// <param name="items">待试穿的装备列表。</param>
        /// <param name="glassesID">待试穿的眼镜表行号，为 0 表示不试穿眼镜。</param>
        /// <param name="clearExisting">是否先清空当前试穿室中的已有试穿项。</param>
        /// <param name="saveOutfit">是否开启“保存/删除搭配”模式；为 <see langword="null"/> 时在最终试穿项超过 1 件时自动开启。</param>
        /// <param name="hideOtherEquipment">是否隐藏其他已装备道具；为 <see langword="null"/> 时保持试穿室当前设置不变。</param>
        /// <returns>成功写入至少一件有效装备或眼镜时返回 <see langword="true"/>。</returns>
        public bool TryOnOutfit
        (
            IReadOnlyList<TryOnEntry> items,
            ushort                    glassesID          = 0,
            bool                      clearExisting      = true,
            bool?                     saveOutfit         = null,
            bool?                     hideOtherEquipment = null
        ) =>
            Unsafe.As<AgentTryon, AgentTryonExpansion>(ref agent).TryOnOutfit
            (
                items,
                glassesID,
                clearExisting,
                saveOutfit,
                hideOtherEquipment
            );

        /// <summary>
        ///     在试穿室中为指定染色通道实时预览染剂颜色。
        ///     当试穿室已打开且存在非眼镜试穿项时直接更新其染色预览；否则按需回退至玩家当前身穿装备。
        /// </summary>
        /// <param name="stainID">目标染剂 ID。</param>
        /// <param name="channel">染色通道索引（0 为通道 1，1 为通道 2）。</param>
        /// <param name="fallbackToEquipped">当前没有可预览的试穿项时，是否自动载入玩家当前身穿装备进行预览。</param>
        /// <returns>成功应用染色预览到至少一件支持该通道的装备时返回 <see langword="true"/>。</returns>
        public bool PreviewStain
        (
            byte stainID,
            int  channel            = 0,
            bool fallbackToEquipped = true
        ) =>
            Unsafe.As<AgentTryon, AgentTryonExpansion>(ref agent).PreviewStain(stainID, channel, fallbackToEquipped);

        /// <summary>
        ///     清除试穿室中全部装备的染色预览状态并恢复原始染色。
        /// </summary>
        /// <returns>存在被清除的染色预览状态时返回 <see langword="true"/>。</returns>
        public bool ClearDyePreview() =>
            Unsafe.As<AgentTryon, AgentTryonExpansion>(ref agent).ClearDyePreview();
    }

    extension
    (
        scoped ref AgentTryonExpansion agent
    )
    {
        /// <summary>
        ///     在试穿室中批量试穿装备与眼镜。
        /// </summary>
        /// <param name="items">待试穿的装备列表。</param>
        /// <param name="glassesID">待试穿的眼镜表行号，为 0 表示不试穿眼镜。</param>
        /// <param name="clearExisting">是否先清空当前试穿室中的已有试穿项。</param>
        /// <param name="saveOutfit">是否开启“保存/删除搭配”模式；为 <see langword="null"/> 时在最终试穿项超过 1 件时自动开启。</param>
        /// <param name="hideOtherEquipment">是否隐藏其他已装备道具；为 <see langword="null"/> 时保持试穿室当前设置不变。</param>
        /// <returns>成功写入至少一件有效装备或眼镜时返回 <see langword="true"/>。</returns>
        public bool TryOnOutfit
        (
            IReadOnlyList<TryOnEntry> items,
            ushort                    glassesID          = 0,
            bool                      clearExisting      = true,
            bool?                     saveOutfit         = null,
            bool?                     hideOtherEquipment = null
        )
        {
            fixed (AgentTryonExpansion* ptr = &agent)
            {
                if (ptr == null)
                    return false;

                return TryOnOutfitCore
                (
                    ptr,
                    items,
                    glassesID,
                    clearExisting,
                    saveOutfit,
                    hideOtherEquipment,
                    null
                );
            }
        }

        /// <summary>
        ///     在试穿室中为指定染色通道实时预览染剂颜色。
        ///     当试穿室已打开且存在非眼镜试穿项时直接更新其染色预览；否则按需回退至玩家当前身穿装备。
        /// </summary>
        /// <param name="stainID">目标染剂 ID。</param>
        /// <param name="channel">染色通道索引（0 为通道 1，1 为通道 2）。</param>
        /// <param name="fallbackToEquipped">当前没有可预览的试穿项时，是否自动载入玩家当前身穿装备进行预览。</param>
        /// <returns>成功应用染色预览到至少一件支持该通道的装备时返回 <see langword="true"/>。</returns>
        public bool PreviewStain
        (
            byte stainID,
            int  channel            = 0,
            bool fallbackToEquipped = true
        )
        {
            fixed (AgentTryonExpansion* ptr = &agent)
            {
                if (ptr == null ||
                    channel is not (0 or 1))
                    return false;

                var tryOnItems  = ptr->TryOnItemEntries;
                var hasExisting = false;

                if (ptr->IsAgentActive())
                {
                    foreach (var item in tryOnItems)
                    {
                        if (item.ID                != 0 &&
                            item.EquipSlotCategory != GLASSES_EQUIP_SLOT)
                        {
                            hasExisting = true;
                            break;
                        }
                    }
                }

                if (hasExisting)
                {
                    var modified = false;

                    for (var i = 0; i < tryOnItems.Length; i++)
                    {
                        ref var item = ref tryOnItems[i];
                        if (item.ID                == 0 ||
                            item.EquipSlotCategory == GLASSES_EQUIP_SLOT)
                            continue;

                        if (ApplyDyePreviewToItem(ref item, stainID, channel))
                            modified = true;
                    }

                    if (!modified)
                        return false;

                    ptr->CharaView.DoUpdate = true;
                    ptr->RefreshAddon();
                    return true;
                }

                if (!fallbackToEquipped ||
                    !TryCollectEquippedEntries(out var equippedEntries))
                    return false;

                var hasDyeableTarget = false;

                foreach (var entry in equippedEntries)
                {
                    var appearanceID = entry.GlamourID != 0 ?
                                           entry.GlamourID :
                                           entry.ItemID;

                    if (SupportsDyeChannel(appearanceID, channel))
                    {
                        hasDyeableTarget = true;
                        break;
                    }
                }

                if (!hasDyeableTarget)
                    return false;

                return TryOnOutfitCore
                (
                    ptr,
                    equippedEntries,
                    0,
                    true,
                    null,
                    null,
                    new(stainID, channel)
                );
            }
        }

        /// <summary>
        ///     清除试穿室中全部装备的染色预览状态并恢复原始染色。
        /// </summary>
        /// <returns>存在被清除的染色预览状态时返回 <see langword="true"/>。</returns>
        public bool ClearDyePreview()
        {
            fixed (AgentTryonExpansion* ptr = &agent)
            {
                if (ptr == null ||
                    !ptr->IsAgentActive())
                    return false;

                var modified   = false;
                var tryOnItems = ptr->TryOnItemEntries;
                var gearItems  = ptr->GearItemEntries;

                for (var i = 0; i < tryOnItems.Length; i++)
                {
                    if (ResetDyePreview(ref tryOnItems[i]))
                        modified = true;
                }

                for (var i = 0; i < gearItems.Length; i++)
                {
                    if (ResetDyePreview(ref gearItems[i]))
                        modified = true;
                }

                if (!modified)
                    return false;

                ptr->CharaView.DoUpdate = true;
                ptr->RefreshAddon();
                return true;
            }
        }
    }

    private static bool TryOnOutfitCore
    (
        AgentTryonExpansion*      agent,
        IReadOnlyList<TryOnEntry> items,
        ushort                    glassesID,
        bool                      clearExisting,
        bool?                     saveOutfit,
        bool?                     hideOtherEquipment,
        DyePreviewRequest?        dyePreview
    )
    {
        var hasValidGlasses = false;
        var glassesIconID   = 0U;

        if (glassesID != 0 &&
            LuminaGetter.TryGetRow<Glasses>(glassesID, out var glassesRow))
        {
            hasValidGlasses = true;
            glassesIconID   = (uint)glassesRow.Icon;
        }

        var resolvedItems = new List<ResolvedTryOnEntry>(Math.Min(items.Count, MAX_TRYON_ITEMS));

        foreach (var item in items)
        {
            if (TryResolveEntry(item, out var resolved))
                resolvedItems.Add(resolved);
        }

        if (!hasValidGlasses &&
            resolvedItems.Count == 0)
        {
            if (clearExisting &&
                agent->IsAgentActive())
                agent->ClearTryOnItems();

            return false;
        }

        if (!agent->IsAgentActive())
            agent->Show();

        if (hideOtherEquipment.HasValue &&
            agent->CharaView.HideOtherEquipment != hideOtherEquipment.Value)
        {
            agent->CharaView.HideOtherEquipment = hideOtherEquipment.Value;
            agent->CharaView.DoUpdate           = true;

            if (!hideOtherEquipment.Value)
                agent->GearItemsChanged = true;
        }

        var tryOnItems = agent->TryOnItemEntries;

        if (clearExisting)
            ResetTryOnItems(tryOnItems);

        var hasNewWeaponOrShield = false;
        var hasNewHeadgear       = false;
        var hasNewWeapon         = false;

        if (hasValidGlasses)
        {
            RemoveGlassesTryOnItem(tryOnItems);

            var glassesIndex = FindFirstEmptySlot(tryOnItems);

            if (glassesIndex >= 0)
            {
                ref var glassesItem = ref tryOnItems[glassesIndex];
                ResetSingleTryOnItem(out glassesItem);
                glassesItem.EquipSlotCategory = GLASSES_EQUIP_SLOT;
                glassesItem.ID                = glassesID;
                glassesItem.IconID            = glassesIconID;
            }
        }

        foreach (var entry in resolvedItems)
        {
            if (entry.UseExplicitSlot)
                RemoveConflictingTryOnItems(tryOnItems, entry.EquipSlot, entry.BlockedSlotMask);

            var targetIndex = FindFirstEmptySlot(tryOnItems);
            if (targetIndex < 0)
                break;

            ref var tryOnItem = ref tryOnItems[targetIndex];
            ResetSingleTryOnItem(out tryOnItem);

            tryOnItem.ID                = entry.ItemID;
            tryOnItem.GlamourID         = entry.GlamourID;
            tryOnItem.Stain0ID          = entry.Stain0ID;
            tryOnItem.Stain1ID          = entry.Stain1ID;
            tryOnItem.ApplyCompanyCrest = entry.ApplyCompanyCrest;
            tryOnItem.IconID            = entry.IconID;

            if (entry.UseExplicitSlot)
            {
                tryOnItem.EquipSlotCategory        = entry.EquipSlot;
                tryOnItem.GlamourEquipSlotCategory = entry.EquipSlotCategoryRowID;
                tryOnItem.BlockedSlotsBitmask      = entry.BlockedSlotMask;
                tryOnItem.ClassJobID               = entry.ClassJobID;

                switch (entry.EquipSlot)
                {
                    case MAINHAND_EQUIP_SLOT:
                        hasNewWeapon         = true;
                        hasNewWeaponOrShield = true;
                        break;

                    case OFFHAND_EQUIP_SLOT:
                        hasNewWeapon = true;

                        if (entry.IsShield)
                            hasNewWeaponOrShield = true;

                        break;

                    case HEAD_EQUIP_SLOT:
                        hasNewHeadgear = true;
                        break;
                }
            }
            else
            {
                tryOnItem.EquipSlotCategory = UNRESOLVED_EQUIP_SLOT;
            }

            if (dyePreview.HasValue)
                ApplyDyePreviewToItem(ref tryOnItem, dyePreview.Value.StainID, dyePreview.Value.Channel);
        }

        if (hasNewHeadgear &&
            agent->CharaView.HeadgearHidden)
        {
            agent->CharaView.HeadgearHidden = false;
            agent->CharaView.DoUpdate       = true;
        }

        if (hasNewWeapon &&
            agent->CharaView.WeaponHidden)
        {
            agent->CharaView.WeaponHidden = false;
            agent->CharaView.DoUpdate     = true;
        }

        if (hasNewWeaponOrShield &&
            agent->CharaView.DrawWeapon)
        {
            agent->CharaView.DrawWeapon = false;
            agent->CharaView.DoUpdate   = true;
            agent->PendingRedrawWeapon  = true;
        }

        var itemCount = CountActiveTryOnItems(tryOnItems);
        if (saveOutfit.HasValue)
            agent->SaveDeleteOutfit = saveOutfit.Value;
        else if (itemCount > 1)
            agent->SaveDeleteOutfit = true;

        agent->TryOnItemsChanged = true;
        return itemCount > 0;
    }

    private static bool TryResolveEntry
    (
        TryOnEntry             entry,
        out ResolvedTryOnEntry resolved
    )
    {
        resolved = default;

        var itemID = ItemUtil.GetBaseId(entry.ItemID).ItemId;
        if (itemID == 0 ||
            !LuminaGetter.TryGetRow<Item>(itemID, out var itemRow))
            return false;

        var categoryRowID = itemRow.EquipSlotCategory.RowId;
        if (categoryRowID is 0 or > byte.MaxValue ||
            !LuminaGetter.TryGetRow<EquipSlotCategory>(categoryRowID, out var category))
            return false;

        var glamourID = 0U;

        if (entry.GlamourID != 0)
        {
            var candidateGlamourID = ItemUtil.GetBaseId(entry.GlamourID).ItemId;
            if (candidateGlamourID != 0 &&
                LuminaGetter.TryGetRow<Item>(candidateGlamourID, out _))
                glamourID = candidateGlamourID;
        }

        var useExplicitSlot = false;
        var resolvedSlot    = UNRESOLVED_EQUIP_SLOT;

        if (entry.EquipSlot.HasValue)
        {
            var requestedSlot = entry.EquipSlot.Value;

            if (IsValidEquipSlot(requestedSlot) &&
                GetCategorySlotValue(category, requestedSlot) > 0)
            {
                useExplicitSlot = true;
                resolvedSlot    = requestedSlot;
            }
        }

        if (!useExplicitSlot)
        {
            var defaultSlot = GetDefaultEquipSlot(category);
            if (!IsValidEquipSlot(defaultSlot))
                return false;
        }

        var isShield = itemRow.FilterGroup == SHIELD_FILTER_GROUP;
        var classJobID = resolvedSlot switch
        {
            MAINHAND_EQUIP_SLOT              => (byte)itemRow.ClassJobUse.RowId,
            OFFHAND_EQUIP_SLOT when isShield => (byte)1,
            _                                => (byte)0
        };

        resolved = new
        (
            itemID,
            glamourID,
            entry.Stain0ID,
            entry.Stain1ID,
            entry.ApplyCompanyCrest,
            itemRow.Icon,
            useExplicitSlot,
            resolvedSlot,
            (byte)categoryRowID,
            GetBlockedSlotMask(category),
            classJobID,
            isShield
        );
        return true;
    }

    private static bool TryCollectEquippedEntries
    (
        out List<TryOnEntry> entries
    )
    {
        entries = [];

        if (!InventoryType.EquippedItems.TryGetItems(static x => x.ItemId != 0, out var equippedItems) ||
            equippedItems.Count == 0)
            return false;

        var uiState = UIState.Instance();
        var glamourErrorBitmask = uiState != null ?
                                      uiState->CurrentGlamourErrorsBitmask :
                                      (ushort)0;

        foreach (var equipped in equippedItems)
        {
            if (equipped.Slot is < 0 or > byte.MaxValue)
                continue;

            var slot = (byte)equipped.Slot;
            if (!IsValidEquipSlot(slot))
                continue;

            var itemID          = equipped.GetItemId();
            var hasGlamourError = (glamourErrorBitmask & (1 << slot)) != 0;
            var glamourID = hasGlamourError ?
                                0U :
                                equipped.GlamourId;

            entries.Add
            (
                new
                (
                    itemID,
                    glamourID,
                    equipped.Stains[0],
                    equipped.Stains[1],
                    slot
                )
            );
        }

        return entries.Count > 0;
    }

    private static bool ApplyDyePreviewToItem
    (
        ref AgentTryonExpansion.TryOnItem item,
        byte                              stainID,
        int                               channel
    )
    {
        var appearanceID = item.GlamourID != 0 ?
                               item.GlamourID :
                               item.ID;

        if (!SupportsDyeChannel(appearanceID, channel))
            return false;

        if (!item.IsDyePreviewEnabled)
        {
            item.GlamourStain0ID     = item.Stain0ID;
            item.GlamourStain1ID     = item.Stain1ID;
            item.IsDyePreviewEnabled = true;
        }

        if (channel == 0)
            item.GlamourStain0ID = stainID;
        else
            item.GlamourStain1ID = stainID;

        return true;
    }

    private static bool ResetDyePreview
    (
        ref AgentTryonExpansion.TryOnItem item
    )
    {
        if (item.ID == 0 ||
            !item.IsDyePreviewEnabled)
            return false;

        item.GlamourStain0ID     = 0;
        item.GlamourStain1ID     = 0;
        item.IsDyePreviewEnabled = false;
        return true;
    }

    private static bool SupportsDyeChannel
    (
        uint appearanceItemID,
        int  channel
    )
    {
        if (!LuminaGetter.TryGetRow<Item>(appearanceItemID, out var row))
            return false;

        return channel switch
        {
            0 => row.DyeCount >= 1,
            1 => row.DyeCount >= 2,
            _ => false
        };
    }

    private static void ResetTryOnItems
    (
        Span<AgentTryonExpansion.TryOnItem> tryOnItems
    )
    {
        for (var i = 0; i < tryOnItems.Length; i++)
            ResetSingleTryOnItem(out tryOnItems[i]);
    }

    private static void ResetSingleTryOnItem
    (
        out AgentTryonExpansion.TryOnItem item
    )
    {
        item                   = default;
        item.EquipSlotCategory = UNRESOLVED_EQUIP_SLOT;
    }

    private static int FindFirstEmptySlot
    (
        ReadOnlySpan<AgentTryonExpansion.TryOnItem> tryOnItems
    )
    {
        for (var i = 0; i < tryOnItems.Length; i++)
        {
            if (tryOnItems[i].ID == 0)
                return i;
        }

        return -1;
    }

    private static int CountActiveTryOnItems
    (
        ReadOnlySpan<AgentTryonExpansion.TryOnItem> tryOnItems
    )
    {
        var count = 0;

        foreach (var item in tryOnItems)
        {
            if (item.ID != 0)
                count++;
        }

        return count;
    }

    private static void RemoveGlassesTryOnItem
    (
        Span<AgentTryonExpansion.TryOnItem> tryOnItems
    )
    {
        var writeIndex = 0;

        for (var readIndex = 0; readIndex < tryOnItems.Length; readIndex++)
        {
            var current = tryOnItems[readIndex];
            if (current.ID                == 0 ||
                current.EquipSlotCategory == GLASSES_EQUIP_SLOT)
                continue;

            if (writeIndex != readIndex)
                tryOnItems[writeIndex] = current;

            writeIndex++;
        }

        for (var i = writeIndex; i < tryOnItems.Length; i++)
            ResetSingleTryOnItem(out tryOnItems[i]);
    }

    private static void RemoveConflictingTryOnItems
    (
        Span<AgentTryonExpansion.TryOnItem> tryOnItems,
        byte                                equipSlot,
        uint                                blockedMask
    )
    {
        var slotBit    = 1U << equipSlot;
        var writeIndex = 0;

        for (var readIndex = 0; readIndex < tryOnItems.Length; readIndex++)
        {
            var current = tryOnItems[readIndex];
            if (current.ID == 0)
                continue;

            var isConflict = current.EquipSlotCategory != GLASSES_EQUIP_SLOT    &&
                             current.EquipSlotCategory != UNRESOLVED_EQUIP_SLOT &&
                             (current.EquipSlotCategory                                         == equipSlot ||
                              (blockedMask                 & (1U << current.EquipSlotCategory)) != 0         ||
                              (current.BlockedSlotsBitmask & slotBit)                           != 0);
            if (isConflict)
                continue;

            if (writeIndex != readIndex)
                tryOnItems[writeIndex] = current;

            writeIndex++;
        }

        for (var i = writeIndex; i < tryOnItems.Length; i++)
            ResetSingleTryOnItem(out tryOnItems[i]);
    }

    private static bool IsValidEquipSlot
    (
        byte slot
    ) =>
        slot <= FINGER_R_EQUIP_SLOT &&
        slot != WAIST_EQUIP_SLOT;

    private static byte GetDefaultEquipSlot
    (
        EquipSlotCategory category
    )
    {
        for (byte slot = 0; slot < EQUIP_SLOT_COUNT; slot++)
        {
            if (GetCategorySlotValue(category, slot) > 0)
                return slot;
        }

        return UNRESOLVED_EQUIP_SLOT;
    }

    private static uint GetBlockedSlotMask
    (
        EquipSlotCategory category
    )
    {
        var mask = 0U;

        for (byte slot = 0; slot < EQUIP_SLOT_COUNT; slot++)
        {
            if (GetCategorySlotValue(category, slot) < 0)
                mask |= 1U << slot;
        }

        return mask;
    }

    private static sbyte GetCategorySlotValue
    (
        EquipSlotCategory category,
        byte              slot
    ) =>
        slot switch
        {
            0  => category.MainHand,
            1  => category.OffHand,
            2  => category.Head,
            3  => category.Body,
            4  => category.Gloves,
            5  => category.Waist,
            6  => category.Legs,
            7  => category.Feet,
            8  => category.Ears,
            9  => category.Neck,
            10 => category.Wrists,
            11 => category.FingerL,
            12 => category.FingerR,
            13 => category.SoulCrystal,
            _  => 0
        };

    private readonly record struct DyePreviewRequest
    (
        byte StainID,
        int  Channel
    );

    private readonly record struct ResolvedTryOnEntry
    (
        uint ItemID,
        uint GlamourID,
        byte Stain0ID,
        byte Stain1ID,
        bool ApplyCompanyCrest,
        uint IconID,
        bool UseExplicitSlot,
        byte EquipSlot,
        byte EquipSlotCategoryRowID,
        uint BlockedSlotMask,
        byte ClassJobID,
        bool IsShield
    );

    #region 常量

    private const int MAX_TRYON_ITEMS  = 14;
    private const int EQUIP_SLOT_COUNT = 14;

    private const byte MAINHAND_EQUIP_SLOT   = 0;
    private const byte OFFHAND_EQUIP_SLOT    = 1;
    private const byte HEAD_EQUIP_SLOT       = 2;
    private const byte WAIST_EQUIP_SLOT      = 5;
    private const byte FINGER_R_EQUIP_SLOT   = 12;
    private const byte GLASSES_EQUIP_SLOT    = 13;
    private const byte UNRESOLVED_EQUIP_SLOT = 14;

    private const byte SHIELD_FILTER_GROUP = 3;

    #endregion
}
