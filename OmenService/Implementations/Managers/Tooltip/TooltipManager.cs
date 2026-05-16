using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Enums;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InteropGenerator.Runtime;
using Lumina.Text.ReadOnly;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Models;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public unsafe class TooltipManager : OmenServiceBase<TooltipManager>
{
    #region 公开订阅

    /// <remarks>
    ///     除了 <see cref="ItemKind.EventItem" /> 外, 其余均为处理好的 BaseID, 方便直接查表
    /// </remarks>
    public delegate void ItemTooltipUpdateDelegate(ItemKind itemKind, uint itemID, ref List<TooltipItemModification> modifications);

    public delegate void ActionTooltipUpdateDelegate(DetailKind actionKind, uint actionID, ref List<TooltipActionModification> modifications);

    #endregion

    #region 公开方法

    /// <summary>
    ///     触发一次物品工具信息界面更新
    /// </summary>
    /// <remarks>
    ///     在你需要更新内容时调用
    /// </remarks>
    public void TriggerItemDetailUpdate()
    {
        DService.Instance().Framework.RunOnFrameworkThread
        (() =>
            {
                if (!ItemDetail->IsAddonAndNodesReady()) return;

                var agent = AgentItemDetail.Instance();
                *(byte*)((nint)agent + agentItemDetailRefreshFlagOffset) = 1;

                DLog.Verbose($"{nameof(TooltipManager)}: 触发更新物品工具信息界面");
            }
        );
    }

    /// <summary>
    ///     触发一次技能工具信息界面更新
    /// </summary>
    /// <remarks>
    ///     在你需要更新内容时调用
    /// </remarks>
    public void TriggerActionDetailUpdate()
    {
        DService.Instance().Framework.RunOnFrameworkThread
        (() =>
            {
                if (!ActionDetail->IsAddonAndNodesReady()) return;
                ActionDetail->OnRequestedUpdate
                (
                    AtkStage.Instance()->GetNumberArrayData(),
                    AtkStage.Instance()->GetStringArrayData()
                );

                DLog.Verbose($"{nameof(TooltipManager)}: 触发更新技能工具信息界面");
            }
        );
    }

    /// <summary>
    ///     获取原始物品工具信息文本
    /// </summary>
    /// <remarks>
    ///     请确保在 <see cref="ItemTooltipUpdateDelegate" /> 期间调用
    /// </remarks>
    public ReadOnlySeString GetOriginalItemTooltipText(TooltipItemType target) =>
        itemOriginalTexts[(int)target];

    /// <summary>
    ///     获取原始技能工具信息文本
    /// </summary>
    /// <remarks>
    ///     请确保在 <see cref="ActionTooltipUpdateDelegate" /> 期间调用
    /// </remarks>
    public ReadOnlySeString GetOriginalActionTooltipText(TooltipActionType target) =>
        actionOriginalTexts[(int)target];

    #region 订阅

    public void RegItem(ItemTooltipUpdateDelegate method, params ItemTooltipUpdateDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public void RegAction(ActionTooltipUpdateDelegate method, params ActionTooltipUpdateDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public void Unreg(params ItemTooltipUpdateDelegate[] methods) =>
        UnregisterGeneric(methods);

    public void Unreg(params ActionTooltipUpdateDelegate[] methods) =>
        UnregisterGeneric(methods);

    #endregion

    #endregion

    #region 私有逆向

    private static readonly CompSig AgentItemDetailRefreshFlagOffsetSig = new("88 83 ?? ?? ?? ?? 48 8B 5C 24 ?? 48 8B 6C 24");
    private                 nint    agentItemDetailRefreshFlagOffset;

    #endregion

    #region 私有状态

    // 上个物品
    private (uint ID, ItemKind Kind) lastItemInfo;

    // 物品原始文本
    private ReadOnlySeString[] itemOriginalTexts = new ReadOnlySeString[65];

    // 上个技能
    private (DetailKind Kind, uint ID) lastActionInfo;

    // 技能原始文本
    private ReadOnlySeString[] actionOriginalTexts = new ReadOnlySeString[16];

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    #endregion

    protected override void Init()
    {
        agentItemDetailRefreshFlagOffset = Marshal.ReadInt32(AgentItemDetailRefreshFlagOffsetSig.ScanText() + 2);
        DLog.Debug($"[{nameof(TooltipManager)}] AgnetItemDetail 工具信息界面刷新标志偏移量: {agentItemDetailRefreshFlagOffset}");

        DService.Instance().AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail",   OnItemDetailUpdate);
        DService.Instance().AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ActionDetail", OnActionDetailUpdate);
    }

    protected override void Uninit()
    {
        DService.Instance().AddonLifecycle.UnregisterListener(OnItemDetailUpdate);
        DService.Instance().AddonLifecycle.UnregisterListener(OnActionDetailUpdate);
    }

    // 物品
    private void OnItemDetailUpdate(AddonEvent type, AddonArgs args)
    {
        var stringArrayData = AtkStage.Instance()->GetStringArrayData(StringArrayType.ItemDetail);
        var textArray       = stringArrayData->StringArray;
        var numberArrayData = AtkStage.Instance()->GetNumberArrayData(NumberArrayType.ItemDetail);
        var itemGroupFlags  = GetItemTooltipGroupFlags(numberArrayData);

        var currentItemInfo = GetItemInfo(AgentItemDetail.Instance()->ItemId);

        if (currentItemInfo != lastItemInfo)
        {
            lastItemInfo      = currentItemInfo;
            itemOriginalTexts = new ReadOnlySeString[65];

            DLog.Verbose($"[{nameof(TooltipManager)}] 物品工具提示内容刷新: {lastItemInfo}");

            for (var i = 0; i < itemOriginalTexts.Length; i++)
            {
                if (!IsItemTooltipTextSet(itemGroupFlags, (TooltipItemType)i) || !textArray[i].HasValue)
                {
                    itemOriginalTexts[i] = new();
                    continue;
                }

                itemOriginalTexts[i] = new(new CStringPointer(textArray[i].Value).AsSpan());
            }
        }

        // 改一下
        var addon = (AddonItemDetail*)ItemDetail;
        if (ItemDetail->IsAddonAndNodesReady())
            addon->CategoryText->TextFlags &= ~TextFlags.Ellipsis;

        DLog.Verbose($"[{nameof(TooltipManager)}] 物品工具提示刷新: {lastItemInfo}");

        // 这里是文本修改
        if (!methodsCollection.TryGetValue(typeof(ItemTooltipUpdateDelegate), out var itemDelegates))
            return;

        // 收集
        var modificationsByTarget = new Dictionary
        <
            TooltipItemType,
            (
            List<TooltipItemModification> Prepend,
            List<TooltipItemModification> Body,
            List<TooltipItemModification> Append
            )
        >();

        foreach (var itemDelegate in itemDelegates)
        {
            var tooltipUpdate = (ItemTooltipUpdateDelegate)itemDelegate;

            List<TooltipItemModification> modifications = [];
            tooltipUpdate(currentItemInfo.Kind, currentItemInfo.ID, ref modifications);

            foreach (var modification in modifications)
            {
                if (!modificationsByTarget.TryGetValue(modification.Target, out var targetModifications))
                {
                    targetModifications =
                        (
                            [],
                            [],
                            []
                        );
                    modificationsByTarget[modification.Target] = targetModifications;
                }

                switch (modification.Type)
                {
                    case TooltipModificationType.Prepend:
                        targetModifications.Prepend.Add(modification);
                        break;
                    case TooltipModificationType.Contribute:
                        targetModifications.Body.Add(modification);
                        break;
                    case TooltipModificationType.Append:
                        targetModifications.Append.Add(modification);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(modification.Type));
                }
            }
        }

        // 形成
        foreach (var (target, targetModifications) in modificationsByTarget)
        {
            var index = (int)target;
            if ((uint)index >= (uint)itemOriginalTexts.Length) continue;

            using var rentedBuilder = new RentedSeStringBuilder();
            var       builder       = rentedBuilder.Builder;

            var hasText = false;

            foreach (var modification in targetModifications.Prepend)
            {
                if (hasText)
                    builder.AppendNewLine();

                builder.Append(modification.Text);
                if (!modification.Text.IsEmpty)
                    hasText = true;
            }

            if (targetModifications.Body.Count <= 0)
            {
                if (!itemOriginalTexts[index].IsEmpty)
                {
                    if (hasText)
                        builder.AppendNewLine();

                    builder.Append(itemOriginalTexts[index]);
                    hasText = true;
                }
            }
            else
            {
                foreach (var modification in targetModifications.Body)
                {
                    if (hasText)
                        builder.AppendNewLine();

                    builder.Append(modification.Text);
                    if (!modification.Text.IsEmpty)
                        hasText = true;
                }
            }

            foreach (var modification in targetModifications.Append)
            {
                if (hasText)
                    builder.AppendNewLine();

                builder.Append(modification.Text);
                if (!modification.Text.IsEmpty)
                    hasText = true;
            }

            stringArrayData->SetValue(index, builder.GetViewAsSpan(), suppressUpdates: true);
        }

        SetItemTooltipGroupFlags(numberArrayData, modificationsByTarget.Keys);
    }

    // 技能
    private void OnActionDetailUpdate(AddonEvent type, AddonArgs args)
    {
        var stringArrayData = AtkStage.Instance()->GetStringArrayData(StringArrayType.ActionDetail);
        var textArray       = stringArrayData->StringArray;

        var currentActionInfo = (AgentActionDetail.Instance()->ActionKind, AgentActionDetail.Instance()->ActionId);

        if (currentActionInfo != lastActionInfo)
        {
            lastActionInfo      = currentActionInfo;
            actionOriginalTexts = new ReadOnlySeString[16];

            DLog.Verbose($"[{nameof(TooltipManager)}] 技能工具提示内容刷新: {lastActionInfo}");

            for (var i = 0; i < actionOriginalTexts.Length; i++)
            {
                if (!textArray[i].HasValue)
                {
                    actionOriginalTexts[i] = new ReadOnlySeString();
                    continue;
                }

                actionOriginalTexts[i] = new ReadOnlySeString(new CStringPointer(textArray[i].Value).AsSpan());
            }
        }

        DLog.Verbose($"[{nameof(TooltipManager)}] 物品工具提示刷新: {lastItemInfo}");

        // 这里是文本修改
        if (!methodsCollection.TryGetValue(typeof(ActionTooltipUpdateDelegate), out var actionDelegates))
            return;

        var modificationsByTarget = new Dictionary
        <
            TooltipActionType,
            (
            List<TooltipActionModification> Prepend,
            List<TooltipActionModification> Body,
            List<TooltipActionModification> Append
            )
        >();

        foreach (var actionDelegate in actionDelegates)
        {
            var tooltipUpdate = (ActionTooltipUpdateDelegate)actionDelegate;

            List<TooltipActionModification> modifications = [];
            tooltipUpdate(currentActionInfo.Item1, currentActionInfo.Item2, ref modifications);

            foreach (var modification in modifications)
            {
                if (!modificationsByTarget.TryGetValue(modification.Target, out var targetModifications))
                {
                    targetModifications =
                        (
                            [],
                            [],
                            []
                        );
                    modificationsByTarget[modification.Target] = targetModifications;
                }

                switch (modification.Type)
                {
                    case TooltipModificationType.Prepend:
                        targetModifications.Prepend.Add(modification);
                        break;
                    case TooltipModificationType.Contribute:
                        targetModifications.Body.Add(modification);
                        break;
                    case TooltipModificationType.Append:
                        targetModifications.Append.Add(modification);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(modification.Type));
                }
            }
        }

        foreach (var (target, targetModifications) in modificationsByTarget)
        {
            var index = (int)target;
            if ((uint)index >= (uint)actionOriginalTexts.Length) continue;

            using var rentedBuilder = new RentedSeStringBuilder();

            var builder = rentedBuilder.Builder;
            var hasText = false;

            foreach (var modification in targetModifications.Prepend)
            {
                if (hasText)
                    builder.AppendNewLine();

                builder.Append(modification.Text);
                if (!modification.Text.IsEmpty)
                    hasText = true;
            }

            if (targetModifications.Body.Count <= 0)
            {
                if (!actionOriginalTexts[index].IsEmpty)
                {
                    if (hasText)
                        builder.AppendNewLine();

                    builder.Append(actionOriginalTexts[index]);
                    hasText = true;
                }
            }
            else
            {
                foreach (var modification in targetModifications.Body)
                {
                    if (hasText)
                        builder.AppendNewLine();

                    builder.Append(modification.Text);
                    if (!modification.Text.IsEmpty)
                        hasText = true;
                }
            }

            foreach (var modification in targetModifications.Append)
            {
                if (hasText)
                    builder.AppendNewLine();

                builder.Append(modification.Text);
                if (!modification.Text.IsEmpty)
                    hasText = true;
            }

            stringArrayData->SetValue(index, builder.GetViewAsSpan(), suppressUpdates: true);
        }
    }

    // 注册
    private bool RegisterGeneric<T>(T method, params T[] methods) where T : Delegate
    {
        var type = typeof(T);

        methodsCollection.AddOrUpdate
        (
            type,
            _ =>
            {
                var list = ImmutableList.Create<Delegate>(method);
                return methods.Length > 0 ? list.AddRange(methods) : list;
            },
            (_, currentList) =>
            {
                var newList = currentList.Add(method);
                return methods.Length > 0 ? newList.AddRange(methods) : newList;
            }
        );

        return true;
    }

    // 取消注册
    private bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        if (methods is not { Length: > 0 }) return false;

        var type = typeof(T);

        while (methodsCollection.TryGetValue(type, out var currentList))
        {
            var newList = currentList.RemoveRange(methods);

            if (newList == currentList)
                return false;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<Type, ImmutableList<Delegate>>(type, currentList);
                if (((ICollection<KeyValuePair<Type, ImmutableList<Delegate>>>)methodsCollection).Remove(kvp))
                    return true;
            }
            else
            {
                if (methodsCollection.TryUpdate(type, newList, currentList))
                    return true;
            }
        }

        return false;
    }

    #region 工具

    private static (uint ID, ItemKind Kind) GetItemInfo(uint itemID)
    {
        switch (itemID)
        {
            // Event Item
            case > 200_0000:
                return (itemID, ItemKind.EventItem);

            // HQ
            case > 100_0000:
                itemID %= 100_0000;
                return (itemID, ItemKind.Hq);

            // 收藏品
            case > 50_0000:
                itemID %= 50_0000;
                return (itemID, ItemKind.Collectible);
        }

        return (itemID, ItemKind.Normal);
    }

    private static TooltipItemGroupFlags GetItemTooltipGroupFlags(NumberArrayData* numberArrayData) =>
        (TooltipItemGroupFlags)numberArrayData->IntArray[5];

    private static bool IsItemTooltipTextSet(TooltipItemGroupFlags flags, TooltipItemType target)
    {
        var isHeaderStatsMode = (flags & TooltipItemGroupFlags.HeaderStatsGroup) != 0;

        return target switch
        {
            TooltipItemType.Name
                or TooltipItemType.GlamourName
                or TooltipItemType.UICategory
                or TooltipItemType.OwnedCount
                or TooltipItemType.ItemLevel
                or TooltipItemType.ControlHelp => true,

            TooltipItemType.MainParam0Name
                or TooltipItemType.MainParam1Name
                or TooltipItemType.MainParam2Name
                or TooltipItemType.MainParam0Value
                or TooltipItemType.MainParam1Value
                or TooltipItemType.MainParam2Value
                or TooltipItemType.MainParam0OffsetValue
                or TooltipItemType.MainParam1OffsetValue
                or TooltipItemType.MainParam2OffsetValue => isHeaderStatsMode,

            TooltipItemType.MarkerName  => (flags & TooltipItemGroupFlags.CrafterName) != 0,
            TooltipItemType.Description => (flags & TooltipItemGroupFlags.Description) != 0,
            TooltipItemType.SellInfo    => (flags & TooltipItemGroupFlags.Marketable)  != 0,

            TooltipItemType.ClassJobCategory
                or TooltipItemType.ClassJobLevel => (flags &
                                                     (isHeaderStatsMode
                                                          ? TooltipItemGroupFlags.EquipRestrictionHeader
                                                          : TooltipItemGroupFlags.EquipRestriction)) !=
                                                    0,

            TooltipItemType.EffectTitle
                or TooltipItemType.Effect => (flags & TooltipItemGroupFlags.Bonuses) != 0,

            TooltipItemType.SpecialTitle
                or TooltipItemType.SpecialParam0
                or TooltipItemType.SpecialParam1
                or TooltipItemType.SpecialParam2
                or TooltipItemType.SpecialParam3
                or TooltipItemType.SpecialParam4 => isHeaderStatsMode && (flags & TooltipItemGroupFlags.Effects) != 0,

            TooltipItemType.AttachMateriaTitle
                or TooltipItemType.AttachableGearCategory
                or TooltipItemType.AttachableGearContent
                or TooltipItemType.MateriaTitle
                or TooltipItemType.AttachedMateria0
                or TooltipItemType.AttachedMateria1
                or TooltipItemType.AttachedMateria2
                or TooltipItemType.AttachedMateria3
                or TooltipItemType.AttachedMateria4
                or TooltipItemType.AttachedMateria0Param
                or TooltipItemType.AttachedMateria1Param
                or TooltipItemType.AttachedMateria2Param
                or TooltipItemType.AttachedMateria3Param
                or TooltipItemType.AttachedMateria4Param => (flags & TooltipItemGroupFlags.Materia) != 0,

            TooltipItemType.DurabilityValue
                or TooltipItemType.SpiritbondCategory
                or TooltipItemType.SpiritbondValue
                or TooltipItemType.RepairInfo
                or TooltipItemType.RepairMaterial
                or TooltipItemType.QuickRepairCost
                or TooltipItemType.AttachMateriaInfo
                or TooltipItemType.GearAbilityInfo => (flags & TooltipItemGroupFlags.CraftingAndRepairs) != 0,

            TooltipItemType.ShopInfo => (flags & TooltipItemGroupFlags.ShopSellingPrice) != 0,

            _ => false
        };
    }

    private static void SetItemTooltipGroupFlags(NumberArrayData* numberArrayData, IEnumerable<TooltipItemType> modifiedTargets)
    {
        var                   isHeaderStatsMode = (GetItemTooltipGroupFlags(numberArrayData) & TooltipItemGroupFlags.HeaderStatsGroup) != 0;
        TooltipItemGroupFlags flagsToSet        = 0;

        foreach (var target in modifiedTargets)
        {
            switch (target)
            {
                case TooltipItemType.MarkerName:
                    flagsToSet |= TooltipItemGroupFlags.CrafterName;
                    break;
                case TooltipItemType.Description:
                    flagsToSet |= TooltipItemGroupFlags.Description;
                    break;
                case TooltipItemType.SellInfo:
                    flagsToSet |= TooltipItemGroupFlags.Marketable;
                    break;
                case TooltipItemType.ClassJobCategory:
                case TooltipItemType.ClassJobLevel:
                    flagsToSet |= isHeaderStatsMode
                                      ? TooltipItemGroupFlags.EquipRestrictionHeader
                                      : TooltipItemGroupFlags.EquipRestriction;
                    break;
                case TooltipItemType.EffectTitle:
                case TooltipItemType.Effect:
                    flagsToSet |= TooltipItemGroupFlags.Bonuses;
                    break;
                case TooltipItemType.SpecialTitle:
                case TooltipItemType.SpecialParam0:
                case TooltipItemType.SpecialParam1:
                case TooltipItemType.SpecialParam2:
                case TooltipItemType.SpecialParam3:
                case TooltipItemType.SpecialParam4:
                    if (isHeaderStatsMode)
                        flagsToSet |= TooltipItemGroupFlags.Effects;
                    break;
                case TooltipItemType.AttachMateriaTitle:
                case TooltipItemType.AttachableGearCategory:
                case TooltipItemType.AttachableGearContent:
                case TooltipItemType.MateriaTitle:
                case TooltipItemType.AttachedMateria0:
                case TooltipItemType.AttachedMateria1:
                case TooltipItemType.AttachedMateria2:
                case TooltipItemType.AttachedMateria3:
                case TooltipItemType.AttachedMateria4:
                case TooltipItemType.AttachedMateria0Param:
                case TooltipItemType.AttachedMateria1Param:
                case TooltipItemType.AttachedMateria2Param:
                case TooltipItemType.AttachedMateria3Param:
                case TooltipItemType.AttachedMateria4Param:
                    flagsToSet |= TooltipItemGroupFlags.Materia;
                    break;
                case TooltipItemType.DurabilityValue:
                case TooltipItemType.SpiritbondCategory:
                case TooltipItemType.SpiritbondValue:
                case TooltipItemType.RepairInfo:
                case TooltipItemType.RepairMaterial:
                case TooltipItemType.QuickRepairCost:
                case TooltipItemType.AttachMateriaInfo:
                case TooltipItemType.GearAbilityInfo:
                    flagsToSet |= TooltipItemGroupFlags.CraftingAndRepairs;
                    break;
                case TooltipItemType.ShopInfo:
                    flagsToSet |= TooltipItemGroupFlags.ShopSellingPrice;
                    break;
            }
        }

        if (flagsToSet != 0)
            numberArrayData->IntArray[5] |= (int)flagsToSet;
    }

    #endregion
}
