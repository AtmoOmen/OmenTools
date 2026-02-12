using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InteropGenerator.Runtime;
using Lumina.Excel.Sheets;
using OmenTools.Abstracts;
using ActionKind = FFXIVClientStructs.FFXIV.Client.UI.Agent.ActionKind;
using RowStatus = Lumina.Excel.Sheets.Status;

namespace OmenTools.Managers;

public unsafe class GameTooltipManager : OmenServiceBase<GameTooltipManager>
{
    #region 外部委托

    public delegate void ActionTooltipModifierDelegate(AtkUnitBase* addon, NumberArrayData* numberArray, StringArrayData* stringArray);

    public delegate void GenerateItemTooltipModifierDelegate(AtkUnitBase* addon, NumberArrayData* numberArray, StringArrayData* stringArray);

    public delegate void GenerateActionTooltipModifierDelegate(AtkUnitBase* addon, NumberArrayData* numberArray, StringArrayData* stringArray);

    public delegate void TooltipShowModifierDelegate
    (
        AtkTooltipManager*                manager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* args
    );

    #endregion

    #region 私有字段

    private static readonly CompSig GenerateItemTooltipSig = new("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8B 42 ?? 4C 8B EA");
    private delegate void* GenerateItemTooltipDelegate
    (
        AtkUnitBase*     addon,
        NumberArrayData* numberArrayData,
        StringArrayData* stringArrayData
    );
    private Hook<GenerateItemTooltipDelegate>? GenerateItemTooltipHook;

    private static readonly CompSig GenerateActionTooltipSig = new("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8B 42 ?? 4C 8B FA");
    private delegate void* GenerateActionTooltipDelegate
    (
        AtkUnitBase*     addon,
        NumberArrayData* numberArrayData,
        StringArrayData* stringArrayData
    );
    private Hook<GenerateActionTooltipDelegate>? GenerateActionTooltipHook;

    private static readonly CompSig HandleActionHoverSig = new("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC ?? 45 8B F1 41 8B D8");
    private delegate void HandleActionHoverDelegate
    (
        AgentActionDetail* agent,
        ActionKind         actionKind,
        uint               actionID,
        int                flag,
        byte               isLovmActionDetail
    );
    private Hook<HandleActionHoverDelegate>? HandleActionHoverHook;

    private static readonly CompSig ShowTooltipSig = new("66 44 89 44 24 ?? 55 53 41 54");
    private delegate void ShowTooltipDelegate
    (
        AtkTooltipManager*                manager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* tooltipArgs,
        void*                             unkDelegate,
        byte                              unk7,
        byte                              unk8
    );
    private Hook<ShowTooltipDelegate>? ShowTooltipHook;

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>>                         modifiers            = [];
    private readonly ConcurrentDictionary<TooltipModifyType, ImmutableList<TooltipModification>> tooltipModifications = [];

    private readonly TooltipActionDetail hoveredActionDetail = new();
    private          SeString            weatherTooltipText  = SeString.Empty;

    #endregion

    internal override void Init()
    {
        GenerateItemTooltipHook ??= GenerateItemTooltipSig.GetHook<GenerateItemTooltipDelegate>(GenerateItemTooltipDetour);
        GenerateItemTooltipHook.Enable();

        GenerateActionTooltipHook ??= GenerateActionTooltipSig.GetHook<GenerateActionTooltipDelegate>(GenerateActionTooltipDetour);
        GenerateActionTooltipHook.Enable();

        HandleActionHoverHook ??= HandleActionHoverSig.GetHook<HandleActionHoverDelegate>(HandleActionHoverDetour);
        HandleActionHoverHook.Enable();

        DService.Instance().AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ActionDetail", OnActionDetailRequestedUpdate);

        ShowTooltipHook ??= ShowTooltipSig.GetHook<ShowTooltipDelegate>(ShowTooltipDetour);
        ShowTooltipHook.Enable();
    }

    internal override void Uninit()
    {
        GenerateItemTooltipHook?.Dispose();
        GenerateItemTooltipHook = null;

        GenerateActionTooltipHook?.Dispose();
        GenerateActionTooltipHook = null;

        DService.Instance().AddonLifecycle.UnregisterListener(OnActionDetailRequestedUpdate);

        HandleActionHoverHook?.Dispose();
        HandleActionHoverHook = null;

        ShowTooltipHook?.Dispose();
        ShowTooltipHook = null;

        modifiers.Clear();
        tooltipModifications.Clear();
    }

    #region 公共接口

    public SeString GetShowenWeatherTooltip() =>
        weatherTooltipText;

    #endregion

    #region 工具方法

    private static void SetSeStringToCStringPointer(ref CStringPointer text, SeString seString)
    {
        var bytes = seString.EncodeWithNullTerminator();
        var ptr   = (byte*)Marshal.AllocHGlobal(bytes.Length);

        for (var i = 0; i < bytes.Length; i++)
            ptr[i] = bytes[i];

        text = ptr;
    }

    private static SeString GetSeStringFromCStringPointer(CStringPointer cStringPointer) =>
        MemoryHelper.ReadSeStringNullTerminated((nint)cStringPointer.Value);

    private static void SetItemTooltipString(StringArrayData* stringArrayData, TooltipItemType type, SeString seString)
    {
        seString ??= new SeString();
        var bytes = seString.EncodeWithNullTerminator();
        stringArrayData->SetValue((int)type, bytes, false);
    }

    private static void SetActionTooltipString(StringArrayData* stringArrayData, TooltipActionType type, SeString seString)
    {
        seString ??= new SeString();
        var bytes = seString.EncodeWithNullTerminator();
        stringArrayData->SetValue((int)type, bytes, false);
    }

    private static SeString ApplyModifications
    (
        SeString                         currentText,
        IEnumerable<TooltipModification> modifications,
        Func<TooltipModification, bool>? extraCondition = null
    )
    {
        var finalText = currentText;

        foreach (var modification in modifications)
        {
            if (extraCondition != null && !extraCondition(modification))
                continue;

            if (currentText.ToString().Contains(modification.Text.ToString()))
                continue;

            switch (modification.Mode)
            {
                case TooltipModifyMode.Overwrite:
                    finalText = modification.Text;
                    break;

                case TooltipModifyMode.Prepend:
                    finalText = new SeString().Append(modification.Text).Append(currentText);
                    break;

                case TooltipModifyMode.Append:
                    finalText = finalText.Append(modification.Text);
                    break;

                case TooltipModifyMode.Regex:
                    if (!string.IsNullOrEmpty(modification.RegexPattern) && modification.Text != null)
                    {
                        try
                        {
                            var regex = new Regex(modification.RegexPattern);
                            finalText = new SeString().Append(regex.Replace(currentText.TextValue, match => match.Groups[1].Value + modification.Text.TextValue));
                        }
                        catch
                        {
                            //ingored
                        }
                    }

                    break;
            }
        }

        return finalText;
    }

    private static void AddStatusesToMap(StatusManager statusesManager, ref Dictionary<uint, uint> map)
    {
        foreach (var statuse in statusesManager.Status)
        {
            if (statuse.StatusId == 0) continue;
            if (!LuminaGetter.TryGetRow<RowStatus>(statuse.StatusId, out var status))
                continue;

            map.TryAdd(status.Icon, status.RowId);

            for (var i = 1; i <= statuse.Param; i++)
                map.TryAdd((uint)(status.Icon + i), status.RowId);
        }
    }

    #endregion

    #region 注册/注销接口

    private void AddGeneric(TooltipModifyType type, TooltipModification modification) =>
        tooltipModifications.AddOrUpdate
        (
            type,
            _ => ImmutableList.Create(modification),
            (_, currentList) => currentList.Add(modification)
        );

    private bool RemoveGeneric(TooltipModifyType type, params TooltipModification[] modifications)
    {
        if (modifications is not { Length: > 0 }) return false;

        while (tooltipModifications.TryGetValue(type, out var currentList))
        {
            var newList = currentList.RemoveRange(modifications);

            if (newList == currentList)
                return false;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<TooltipModifyType, ImmutableList<TooltipModification>>(type, currentList);
                if (((ICollection<KeyValuePair<TooltipModifyType, ImmutableList<TooltipModification>>>)tooltipModifications).Remove(kvp))
                    return true;
            }
            else
            {
                if (tooltipModifications.TryUpdate(type, newList, currentList))
                    return true;
            }
        }

        return false;
    }

    public TooltipModification AddItemDetail
    (
        uint              itemID,
        TooltipItemType   type,
        SeString          text,
        TooltipModifyMode mode = TooltipModifyMode.Overwrite
    )
    {
        var modification = new TooltipModification
        {
            ItemID    = itemID,
            ItemField = type,
            Mode      = mode,
            Text      = text
        };

        AddGeneric(TooltipModifyType.ItemDetail, modification);
        return modification;
    }

    public TooltipModification AddActionDetail
    (
        uint              actionID,
        TooltipActionType type,
        SeString          text,
        TooltipModifyMode mode = TooltipModifyMode.Overwrite
    )
    {
        var modification = new TooltipModification
        {
            ActionID    = actionID,
            ActionField = type,
            Mode        = mode,
            Text        = text
        };

        AddGeneric(TooltipModifyType.ActionDetail, modification);
        return modification;
    }

    public TooltipModification AddWeatherTooltipModify
    (
        SeString          text,
        TooltipModifyMode mode         = TooltipModifyMode.Overwrite,
        string            regexPattern = null
    )
    {
        var modification = new TooltipModification
        {
            Mode         = mode,
            Text         = text,
            RegexPattern = regexPattern
        };

        AddGeneric(TooltipModifyType.Weather, modification);
        return modification;
    }

    public TooltipModification AddStatus
    (
        uint              statuID,
        SeString          text,
        TooltipModifyMode mode         = TooltipModifyMode.Overwrite,
        string            regexPattern = null
    )
    {
        var modification = new TooltipModification
        {
            StatuID      = statuID,
            Mode         = mode,
            Text         = text,
            RegexPattern = regexPattern
        };

        AddGeneric(TooltipModifyType.Status, modification);
        return modification;
    }

    public bool RemoveStatus(params TooltipModification[] modification) =>
        RemoveGeneric(TooltipModifyType.Status, modification);

    public bool RemoveWeather(params TooltipModification[] modification) =>
        RemoveGeneric(TooltipModifyType.Weather, modification);

    public bool RemoveActionDetail(params TooltipModification[] modification) =>
        RemoveGeneric(TooltipModifyType.ActionDetail, modification);

    public bool RemoveItemDetail(params TooltipModification[] modification) =>
        RemoveGeneric(TooltipModifyType.ItemDetail, modification);


    private bool RegisterGeneric<T>(T method, params T[] methods) where T : Delegate
    {
        var type = typeof(T);

        modifiers.AddOrUpdate
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

    private bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        if (methods is not { Length: > 0 }) return false;

        var type = typeof(T);

        while (modifiers.TryGetValue(type, out var currentList))
        {
            var newList = currentList.RemoveRange(methods);

            if (newList == currentList)
                return false;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<Type, ImmutableList<Delegate>>(type, currentList);
                if (((ICollection<KeyValuePair<Type, ImmutableList<Delegate>>>)modifiers).Remove(kvp))
                    return true;
            }
            else
            {
                if (modifiers.TryUpdate(type, newList, currentList))
                    return true;
            }
        }

        return false;
    }


    public bool RegTooltipShowModifier(TooltipShowModifierDelegate method, params TooltipShowModifierDelegate[] methods)
        => RegisterGeneric(method, methods);

    public bool RegGenerateItemTooltipModifier(GenerateItemTooltipModifierDelegate method, params GenerateItemTooltipModifierDelegate[] methods)
        => RegisterGeneric(method, methods);

    public bool RegGenerateActionTooltipModifier(GenerateActionTooltipModifierDelegate method, params GenerateActionTooltipModifierDelegate[] methods)
        => RegisterGeneric(method, methods);

    public bool RegActionTooltipModifier(ActionTooltipModifierDelegate method, params ActionTooltipModifierDelegate[] methods)
        => RegisterGeneric(method, methods);

    public bool Unreg(params TooltipShowModifierDelegate[] itemModifiers) => UnregisterGeneric(itemModifiers);

    public bool Unreg(params GenerateItemTooltipModifierDelegate[] generateItemModifiers) => UnregisterGeneric(generateItemModifiers);

    public bool Unreg(params GenerateActionTooltipModifierDelegate[] generateActionModifiers) => UnregisterGeneric(generateActionModifiers);

    public bool Unreg(params ActionTooltipModifierDelegate[] actionModifiers) => UnregisterGeneric(actionModifiers);

    #endregion

    #region 事件处理
    
    private void HandleActionHoverDetour(AgentActionDetail* agent, ActionKind actionKind, uint actionID, int flag, byte isLovmActionDetail)
    {
        hoveredActionDetail.Category           = actionKind;
        hoveredActionDetail.ID                 = actionID;
        hoveredActionDetail.Flag               = flag;
        hoveredActionDetail.IsLovmActionDetail = isLovmActionDetail != 0;
        HandleActionHoverHook?.Original(agent, actionKind, actionID, flag, isLovmActionDetail);
    }
    
    
    private void* GenerateItemTooltipDetour(AtkUnitBase* addon, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (modifiers.TryGetValue(typeof(GenerateItemTooltipModifierDelegate), out var bag))
        {
            foreach (var modifier in bag)
            {
                var modifierDelegate = (GenerateItemTooltipModifierDelegate)modifier;

                try
                {
                    modifierDelegate(addon, numberArrayData, stringArrayData);
                }
                catch
                {
                    // ignored
                }
            }
        }
        
        var itemID = AgentItemDetail.Instance()->ItemId;
        if (itemID < 2000000)
            itemID %= 500000;

        var modifications = tooltipModifications.GetValueOrDefault(TooltipModifyType.ItemDetail, [])
                                                .Where(m => m.ItemID == itemID)
                                                .ToList();
        if (modifications.Count != 0)
        {
            var modificationsByField = modifications.GroupBy(m => m.ItemField);
            foreach (var group in modificationsByField)
            {
                try
                {
                    var field      = group.Key;
                    var fieldIndex = (byte)field;
                    if (fieldIndex >= stringArrayData->Size) continue;

                    var currentText = GetSeStringFromCStringPointer(stringArrayData->StringArray[fieldIndex]);

                    var finalText = ApplyModifications(currentText, group);

                    SetItemTooltipString(stringArrayData, field, finalText);
                }
                catch
                {
                    // ignored
                }
            }
        }

        return GenerateItemTooltipHook.Original(addon, numberArrayData, stringArrayData);
    }
    
    private void* GenerateActionTooltipDetour(AtkUnitBase* addon, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (modifiers.TryGetValue(typeof(GenerateActionTooltipModifierDelegate), out var bag))
        {
            foreach (var modifier in bag)
            {
                try
                {
                    ((GenerateActionTooltipModifierDelegate)modifier)(addon, numberArrayData, stringArrayData);
                }
                catch
                {
                    // ignored
                }
            }
        }

        var actionID = AgentActionDetail.Instance()->ActionId;
        var modifications = tooltipModifications.GetValueOrDefault(TooltipModifyType.ActionDetail, [])
                                                .Where(m => m.ActionID == actionID)
                                                .ToList();
        if (modifications.Count != 0)
        {
            var modificationsByField = modifications.GroupBy(m => m.ActionField);
            foreach (var group in modificationsByField)
            {
                try
                {
                    var field      = group.Key;
                    var fieldIndex = (byte)field;
                    if (fieldIndex >= stringArrayData->Size) continue;

                    var currentText = GetSeStringFromCStringPointer(stringArrayData->StringArray[fieldIndex]);
                    var finalText   = ApplyModifications(currentText, group);
                    SetActionTooltipString(stringArrayData, field, finalText);
                }
                catch
                {
                    // ignored
                }
            }
        }
        
        return GenerateActionTooltipHook.Original(addon, numberArrayData, stringArrayData);
    }
    
    private void OnActionDetailRequestedUpdate(AddonEvent type, AddonArgs args)
    {
        if (!modifiers.TryGetValue(typeof(ActionTooltipModifierDelegate), out var bag)) return;

        var argsFormat = args as AddonRequestedUpdateArgs;
        
        foreach (var modifier in bag)
        {
            try
            {
                ((ActionTooltipModifierDelegate)modifier)
                (
                    args.Addon.ToStruct(),
                    (NumberArrayData*)argsFormat.NumberArrayData,
                    (StringArrayData*)argsFormat.StringArrayData
                );
            }
            catch
            {
                // ignored
            }
        }
    }

    private void ShowTooltipDetour
    (
        AtkTooltipManager*                manager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* args,
        void*                             unkDelegate,
        byte                              unk7,
        byte                              unk8
    )
    {
        if (modifiers.TryGetValue(typeof(TooltipShowModifierDelegate), out var bag))
        {
            foreach (var modifier in bag)
            {
                try
                {
                    ((TooltipShowModifierDelegate)modifier)(manager, type, parentID, targetNode, args);
                }
                catch
                {
                    // ignored
                }
            }
        }

        // 天气
        try
        {
            if (targetNode != null && NaviMap != null && parentID == NaviMap->Id)
            {
                var compNode = targetNode->ParentNode->GetAsAtkComponentNode();

                if (compNode != null)
                {
                    var imageNode = compNode->Component->UldManager.SearchNodeById(3)->GetAsAtkImageNode();

                    if (imageNode != null)
                    {
                        var iconID    = imageNode->PartsList->Parts[imageNode->PartId].UldAsset->AtkTexture.Resource->IconId;
                        var weatherID = WeatherManager.Instance()->WeatherId;

                        if (LuminaGetter.TryGetRow<Weather>(weatherID, out var weather))
                        {
                            if (weather.Icon == iconID)
                            {
                                var currentText = GetSeStringFromCStringPointer(args->TextArgs.Text);
                                var finalText   = ApplyModifications(currentText, tooltipModifications.GetValueOrDefault(TooltipModifyType.Weather, []));

                                SetSeStringToCStringPointer(ref args->TextArgs.Text, finalText);
                                weatherTooltipText = finalText;
                            }

                        }
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
        
        // 状态效果
        try
        {
            Dictionary<uint, uint> iconStatusIDMap = [];

            if (DService.Instance().ObjectTable.LocalPlayer is { } localPlayer && targetNode != null)
            {
                var imageNode = targetNode->GetAsAtkImageNode();
                if (imageNode != null)
                {
                    var iconID = imageNode->PartsList->Parts[imageNode->PartId].UldAsset->AtkTexture.Resource->IconId;
                    if (iconID is >= 210000 and <= 230000)
                    {
                        if (args->TextArgs.Text.Value != null)
                        {
                            var currentTarget = TargetManager.Target;
                            if (currentTarget != null && currentTarget.Address != localPlayer.Address)
                                AddStatusesToMap(currentTarget.ToBCStruct()->StatusManager, ref iconStatusIDMap);

                            var focusTarget = TargetManager.FocusTarget;
                            if (focusTarget != null)
                                AddStatusesToMap(focusTarget.ToBCStruct()->StatusManager, ref iconStatusIDMap);

                            var partyList = AgentHUD.Instance()->PartyMembers;
                            foreach (var member in partyList.ToArray().Where(m => m.Index != 0))
                            {
                                if (member.Object != null)
                                    AddStatusesToMap(member.Object->StatusManager, ref iconStatusIDMap);
                            }

                            AddStatusesToMap(localPlayer.ToBCStruct()->StatusManager, ref iconStatusIDMap);

                            var currentText = GetSeStringFromCStringPointer(args->TextArgs.Text);
                            var finalText = ApplyModifications
                            (
                                currentText,
                                tooltipModifications.GetValueOrDefault(TooltipModifyType.Status, []),
                                mod => iconStatusIDMap.TryGetValue(iconID, out var statuId) && statuId == mod.StatuID
                            );

                            SetSeStringToCStringPointer(ref args->TextArgs.Text, finalText);
                        }
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        ShowTooltipHook?.Original(manager, type, parentID, targetNode, args, unkDelegate, unk7, unk8);
    }

    #endregion
}

#region 自定义类

public class TooltipModification
{
    public uint              ItemID             { get; init; }
    public uint              ActionID           { get; init; }
    public uint              StatuID            { get; init; }
    public TooltipItemType   ItemField          { get; init; }
    public TooltipActionType ActionField        { get; init; }
    public TooltipModifyMode Mode               { get; init; }
    public SeString          Text               { get; init; }
    public string            RegexPattern       { get; init; }
    public string            ReplacementPattern { get; init; }
}

public class TooltipActionDetail
{
    public ActionKind Category;
    public uint       ID;
    public int        Flag;
    public bool       IsLovmActionDetail;
}

public enum TooltipModifyMode
{
    Overwrite,
    Prepend,
    Append,
    Regex
}

public enum TooltipModifyType
{
    Weather,
    Status,
    ItemDetail,
    ActionDetail
}

public enum TooltipItemType : byte
{
    ItemName,
    GlamourName,
    ItemUICategory,
    ItemDescription                       = 13,
    Effects                               = 16,
    ClassJobCategory                      = 22,
    DurabilityPercent                     = 28,
    SpiritbondPercent                     = 30,
    ExtractableProjectableDesynthesizable = 35,
    Param0                                = 37,
    Param1                                = 38,
    Param2                                = 39,
    Param3                                = 40,
    Param4                                = 41,
    Param5                                = 42,
    ControlsDisplay                       = 64
}

public enum TooltipActionType
{
    ActionName,
    ActionKind,
    Unknown02, // 与ActionKind共享同一位置
    RangeText,
    RangeValue,
    RadiusText,
    RadiusValue,
    MPCostText,
    MPCostValue,
    RecastText,
    RecastValue,
    CastText,
    CastValue,
    Description,
    Level,
    ClassJobAbbr
}

#endregion
