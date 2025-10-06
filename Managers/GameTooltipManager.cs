using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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

public unsafe class GameTooltipManager : OmenServiceBase
{
    #region 外部委托

    public delegate void ActionTooltipModifierDelegate(AtkUnitBase* addon, void* a2, ulong a3);

    public delegate void GenerateItemTooltipModifierDelegate(AtkUnitBase* addon, NumberArrayData* numberArray, StringArrayData* stringArray);

    public delegate void GenerateActionTooltipModifierDelegate(AtkUnitBase* addon, NumberArrayData* numberArray, StringArrayData* stringArray);

    public delegate void TooltipShowModifierDelegate(
        AtkTooltipManager*                manager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* args);

    #endregion

    #region 私有字段

    private static readonly CompSig GenerateItemTooltipSig = new("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8B 42 ?? 4C 8B EA");
    private delegate void* GenerateItemTooltipDelegate(
        AtkUnitBase*     addon,
        NumberArrayData* numberArrayData,
        StringArrayData* stringArrayData);
    private static Hook<GenerateItemTooltipDelegate>? GenerateItemTooltipHook;

    private static readonly CompSig GenerateActionTooltipSig = new("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8B 42 ?? 4C 8B FA");
    private delegate void* GenerateActionTooltipDelegate(
        AtkUnitBase*     addon,
        NumberArrayData* numberArrayData,
        StringArrayData* stringArrayData);
    private static Hook<GenerateActionTooltipDelegate>? GenerateActionTooltipHook;

    private static readonly CompSig                      ActionTooltipSig = new("48 89 5C 24 ?? 55 56 57 41 54 41 56 48 83 EC ?? 48 8B 9A");
    private delegate        nint                         ActionTooltipDelegate(AtkUnitBase* addon, void* a2, ulong a3);
    private static          Hook<ActionTooltipDelegate>? ActionTooltipHook;

    private static readonly CompSig ActionHoveredSig = new("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC ?? 45 8B F1 41 8B D8");
    private delegate void ActionHoveredDelegate(
        AgentActionDetail* agent,
        ActionKind         actionKind,
        uint               actionID,
        int                flag,
        byte               isLovmActionDetail);
    private static Hook<ActionHoveredDelegate>? ActionHoveredHook;

    private static readonly CompSig TooltipShowSig = new("66 44 89 44 24 ?? 55 53 41 54");
    private delegate void TooltipShowDelegate(
        AtkTooltipManager*                atkTooltipManager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* tooltipArgs,
        long                              unkDelegate,
        byte                              unk7,
        byte                              unk8);
    private static Hook<TooltipShowDelegate>? TooltipShowHook;

    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>>                         Modifiers            = [];
    private static readonly ConcurrentDictionary<TooltipModifyType, ConcurrentBag<TooltipModification>> TooltipModifications = [];

    private static readonly TooltipActionDetail HoveredActionDetail = new();
    private static SeString WeatherTooltipText = SeString.Empty;

    #endregion

    internal override void Init()
    {
        GenerateItemTooltipHook ??= GenerateItemTooltipSig.GetHook<GenerateItemTooltipDelegate>(OnGenerateItemTooltipDetour);
        GenerateItemTooltipHook.Enable();

        GenerateActionTooltipHook ??= GenerateActionTooltipSig.GetHook<GenerateActionTooltipDelegate>(OnGenerateActionTooltipDetour);
        GenerateActionTooltipHook.Enable();

        ActionHoveredHook ??= ActionHoveredSig.GetHook<ActionHoveredDelegate>(OnActionHoveredDetour);
        ActionHoveredHook.Enable();

        ActionTooltipHook ??= ActionTooltipSig.GetHook<ActionTooltipDelegate>(OnActionTooltipDetour);
        ActionTooltipHook.Enable();

        TooltipShowHook ??= TooltipShowSig.GetHook<TooltipShowDelegate>(OnTooltipShowDetour);
        TooltipShowHook.Enable();

        RegGenerateItemTooltipModifier(ModifyItemTooltip);
        RegGenerateActionTooltipModifier(ModifyActionTooltip);

        RegTooltipShowModifier(ModifyWeatherTooltip);
        RegTooltipShowModifier(ModifyStatuTooltip);
    }

    #region 公共接口

    public static Guid AddItemDetailTooltipModify(
        uint              itemID,
        TooltipItemType   type,
        SeString          text,
        TooltipModifyMode mode = TooltipModifyMode.Overwrite)
    {
        var modification = new TooltipModification
        {
            ItemID    = itemID,
            ItemField = type,
            Mode      = mode,
            Text      = text,
        };

        var bag = TooltipModifications.GetOrAdd(TooltipModifyType.ItemDetail, _ => []);
        bag.Add(modification);
        return modification.ID;
    }

    public static bool RemoveItemDetailTooltipModify(Guid id) => 
        RemoveTooltipModify(id);

    public static Guid AddActionDetailTooltipModify(
        uint              actionID,
        TooltipActionType type,
        SeString          text,
        TooltipModifyMode mode = TooltipModifyMode.Overwrite)
    {
        var modification = new TooltipModification
        {
            ActionID    = actionID,
            ActionField = type,
            Mode        = mode,
            Text        = text,
        };

        var bag = TooltipModifications.GetOrAdd(TooltipModifyType.ActionDetail, _ => []);
        bag.Add(modification);
        return modification.ID;
    }

    public static bool RemoveActionDetailTooltipModify(Guid id) => 
        RemoveTooltipModify(id);

    public static Guid AddWeatherTooltipModify(SeString text, TooltipModifyMode mode = TooltipModifyMode.Overwrite, string regexPattern = null)
    {
        var modification = new TooltipModification
        {
            Mode = mode,
            Text = text,
            RegexPattern = regexPattern
        };

        var bag = TooltipModifications.GetOrAdd(TooltipModifyType.Weather, _ => []);
        bag.Add(modification);
        return modification.ID;
    }

    public static bool RemoveWeatherTooltipModify(Guid id) => 
        RemoveTooltipModify(id);

    public static Guid AddstatuTooltipModify(uint statuID, SeString text, TooltipModifyMode mode = TooltipModifyMode.Overwrite, string regexPattern = null)
    {
        var modification = new TooltipModification
        {
            StatuID = statuID,
            Mode = mode,
            Text = text,
            RegexPattern = regexPattern
        };

        var bag = TooltipModifications.GetOrAdd(TooltipModifyType.Status, _ => []);
        bag.Add(modification);
        return modification.ID;
    }

    public static bool RemovestatuTooltipModify(Guid id) => 
        RemoveTooltipModify(id);

    public static SeString GetShowenWeatherTooltip() => 
        WeatherTooltipText;

    #endregion

    #region 工具方法

    private static bool RemoveTooltipModify(Guid id)
    {
        foreach (var (type, bag) in TooltipModifications)
        {
            var modification = bag.FirstOrDefault(m => m.ID == id);
            if (modification != null)
            {
                var newBag = new ConcurrentBag<TooltipModification>(bag.Where(m => m.ID != id));
                TooltipModifications[type] = newBag;
                return true;
            }
        }
        
        return false;
    }

    private static void SetSeStringToCStringPointer(ref CStringPointer text, SeString seString)
    {
        var bytes = seString.EncodeWithNullTerminator();
        var ptr = (byte*)Marshal.AllocHGlobal(bytes.Length);

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

    private static SeString ApplyModifications(
        SeString                         currentText,
        IEnumerable<TooltipModification> modifications,
        Func<TooltipModification, bool>? extraCondition = null)
    {
        var finalText = currentText;

        foreach (var modification in modifications)
        {
            if (extraCondition != null && !extraCondition(modification))
                continue;

            if (currentText.ExtractText().Contains(modification.Text.ExtractText()))
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

    private static bool RegisterModifier<T>(params T[] methods) where T : Delegate
    {
        var bag = Modifiers.GetOrAdd(typeof(T), _ => []);
        foreach (var method in methods) 
            bag.Add(method);
        return true;
    }

    private static bool UnregisterModifier<T>(params T[] methods) where T : Delegate
    {
        if (Modifiers.TryGetValue(typeof(T), out var bag))
        {
            var newBag = new ConcurrentBag<Delegate>(bag.Where(d => !methods.Contains(d)));
            Modifiers[typeof(T)] = newBag;
            return true;
        }
        return false;
    }


    public static bool RegTooltipShowModifier(params TooltipShowModifierDelegate[]                     modifiers) => RegisterModifier(modifiers);
    public static bool RegGenerateItemTooltipModifier(GenerateItemTooltipModifierDelegate              modifier)  => RegisterModifier(modifier);    
    public static bool RegGenerateActionTooltipModifier(GenerateActionTooltipModifierDelegate          modifier)  => RegisterModifier(modifier);
    public static bool RegGenerateItemTooltipModifier(params GenerateItemTooltipModifierDelegate[]     modifiers) => RegisterModifier(modifiers);
    public static bool RegActionTooltipModifier(ActionTooltipModifierDelegate                          modifier)  => RegisterModifier(modifier);
    public static bool RegGenerateActionTooltipModifier(params GenerateActionTooltipModifierDelegate[] modifiers) => RegisterModifier(modifiers);
    public static bool RegTooltipShowModifier(TooltipShowModifierDelegate                              modifier)  => RegisterModifier(modifier);
    public static bool RegActionTooltipModifier(params ActionTooltipModifierDelegate[]                 modifiers) => RegisterModifier(modifiers);
    
    public static bool Unreg(params TooltipShowModifierDelegate[]           itemModifiers)           => UnregisterModifier(itemModifiers);
    public static bool Unreg(params GenerateItemTooltipModifierDelegate[]   generateItemModifiers)   => UnregisterModifier(generateItemModifiers);
    public static bool Unreg(params GenerateActionTooltipModifierDelegate[] generateActionModifiers) => UnregisterModifier(generateActionModifiers);
    public static bool Unreg(params ActionTooltipModifierDelegate[]         actionModifiers)         => UnregisterModifier(actionModifiers);

    #endregion

    #region 事件处理

    private static void ModifyItemTooltip(AtkUnitBase* addon, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        try
        {
            var itemID = AgentItemDetail.Instance()->ItemId;
            if (itemID < 2000000)
                itemID %= 500000;

            var modifications = TooltipModifications.GetValueOrDefault(TooltipModifyType.ItemDetail, [])
                                                    .Where(m => m.ItemID == itemID)
                                                    .ToList();
            if (modifications.Count == 0) return;

            // 按字段分组
            var modificationsByField = modifications.GroupBy(m => m.ItemField);

            foreach (var group in modificationsByField)
            {
                var field = group.Key;
                var fieldIndex = (byte)field;
                if (fieldIndex >= stringArrayData->Size) continue;

                var currentText = GetSeStringFromCStringPointer(stringArrayData->StringArray[fieldIndex]);

                var finalText = ApplyModifications(currentText, group);

                SetItemTooltipString(stringArrayData, field, finalText);
            }
        }
        catch
        {
            // Ignored
        }
    }

    private static void ModifyActionTooltip(AtkUnitBase* addon, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        try
        {
            var actionID = AgentActionDetail.Instance()->ActionId;

            var modifications = TooltipModifications.GetValueOrDefault(TooltipModifyType.ActionDetail, [])
                                                    .Where(m => m.ActionID == actionID)
                                                    .ToList();
            if (modifications.Count == 0) return;

            var modificationsByField = modifications.GroupBy(m => m.ActionField);

            foreach (var group in modificationsByField)
            {
                var field = group.Key;
                var fieldIndex = (byte)field;
                if (fieldIndex >= stringArrayData->Size) continue;

                var currentText = GetSeStringFromCStringPointer(stringArrayData->StringArray[fieldIndex]);

                var finalText = ApplyModifications(currentText, group);

                SetActionTooltipString(stringArrayData, field, finalText);
            }
        }
        catch
        {
            // Ignored
        }
    }

    private static void ModifyWeatherTooltip(
        AtkTooltipManager*                manager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* args)
    {
        if (targetNode == null || NaviMap == null || parentID != NaviMap->Id) return;

        var compNode = targetNode->ParentNode->GetAsAtkComponentNode();
        if (compNode == null) return;

        var imageNode = compNode->Component->UldManager.SearchNodeById(3)->GetAsAtkImageNode();
        if (imageNode == null) return;

        var iconID    = imageNode->PartsList->Parts[imageNode->PartId].UldAsset->AtkTexture.Resource->IconId;
        var weatherID = WeatherManager.Instance()->WeatherId;

        if (LuminaGetter.TryGetRow<Weather>(weatherID, out var weather))
        {
            if (weather.Icon != iconID) return;

            var currentText = GetSeStringFromCStringPointer(args->TextArgs.Text);

            var finalText = ApplyModifications(currentText, TooltipModifications.GetValueOrDefault(TooltipModifyType.Weather, []));

            SetSeStringToCStringPointer(ref args->TextArgs.Text, finalText);
            WeatherTooltipText = finalText;
        }
    }

    private static void ModifyStatuTooltip(
        AtkTooltipManager*                manager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* args)
    {
        Dictionary<uint, uint> IconStatusIDMap = [];

        var localPlayer = DService.ObjectTable.LocalPlayer;
        if (localPlayer == null || targetNode == null) return;

        var imageNode = targetNode->GetAsAtkImageNode();
        if (imageNode == null) return;

        var iconID = imageNode->PartsList->Parts[imageNode->PartId].UldAsset->AtkTexture.Resource->IconId;
        if (iconID is < 210000 or > 230000) return;

        if (args->TextArgs.Text.Value == null) return;

        var currentTarget = DService.Targets.Target;
        if (currentTarget != null && currentTarget.Address != localPlayer.Address)
            AddStatusesToMap(currentTarget.ToBCStruct()->StatusManager, ref IconStatusIDMap);

        var focusTarget = DService.Targets.FocusTarget;
        if (focusTarget != null)
            AddStatusesToMap(focusTarget.ToBCStruct()->StatusManager, ref IconStatusIDMap);

        var partyList = AgentHUD.Instance()->PartyMembers;
        foreach (var member in partyList.ToArray().Where(m => m.Index != 0))
        {
            if (member.Object != null)
                AddStatusesToMap(member.Object->StatusManager, ref IconStatusIDMap);
        }

        AddStatusesToMap(localPlayer.ToBCStruct()->StatusManager, ref IconStatusIDMap);

        var currentText = GetSeStringFromCStringPointer(args->TextArgs.Text);

        var finalText = ApplyModifications(
            currentText,
            TooltipModifications.GetValueOrDefault(TooltipModifyType.Status, []),
            mod => IconStatusIDMap.TryGetValue(iconID, out var statuId) && statuId == mod.StatuID
        );

        SetSeStringToCStringPointer(ref args->TextArgs.Text, finalText);
    }

    private static void OnActionHoveredDetour(AgentActionDetail* agent, ActionKind actionKind, uint actionID, int flag, byte isLovmActionDetail)
    {
        HoveredActionDetail.Category = actionKind;
        HoveredActionDetail.ID = actionID;
        HoveredActionDetail.Flag = flag;
        HoveredActionDetail.IsLovmActionDetail = isLovmActionDetail != 0;
        ActionHoveredHook?.Original(agent, actionKind, actionID, flag, isLovmActionDetail);
    }

    private static void* OnGenerateItemTooltipDetour(AtkUnitBase* addon, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (Modifiers.TryGetValue(typeof(GenerateItemTooltipModifierDelegate), out var bag))
        {
            foreach (var modifier in bag.Cast<GenerateItemTooltipModifierDelegate>())
            {
                try
                {
                    modifier(addon, numberArrayData, stringArrayData);
                }
                catch
                {
                    //Ignored
                }
            }
        }

        return GenerateItemTooltipHook.Original(addon, numberArrayData, stringArrayData);
    }

    private static void* OnGenerateActionTooltipDetour(AtkUnitBase* addon, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (Modifiers.TryGetValue(typeof(GenerateActionTooltipModifierDelegate), out var bag))
        {
            foreach (var modifier in bag.Cast<GenerateActionTooltipModifierDelegate>())
            {
                try
                {
                    modifier(addon, numberArrayData, stringArrayData);
                }
                catch
                {
                    //Ignored
                }
            }
        }

        return GenerateActionTooltipHook.Original(addon, numberArrayData, stringArrayData);
    }

    private static nint OnActionTooltipDetour(AtkUnitBase* addon, void* a2, ulong a3)
    {
        if (Modifiers.TryGetValue(typeof(ActionTooltipModifierDelegate), out var bag))
        {
            foreach (var modifier in bag.Cast<ActionTooltipModifierDelegate>())
            {
                try
                {
                    modifier(addon, a2, a3);
                }
                catch
                {
                    //Ignored
                }
            }
        }

        return ActionTooltipHook.Original(addon, a2, a3);
    }

    private static void OnTooltipShowDetour(
        AtkTooltipManager*                manager,
        AtkTooltipManager.AtkTooltipType  type,
        ushort                            parentID,
        AtkResNode*                       targetNode,
        AtkTooltipManager.AtkTooltipArgs* tooltipArgs,
        long                              unkDelegate,
        byte                              unk7,
        byte                              unk8)
    {
        if (Modifiers.TryGetValue(typeof(TooltipShowModifierDelegate), out var bag))
        {
            foreach (var modifier in bag.Cast<TooltipShowModifierDelegate>())
            {
                try
                {
                    modifier(manager, type, parentID, targetNode, tooltipArgs);
                }
                catch
                {
                    // ignored
                }
            }
        }

        TooltipShowHook?.Original(manager, type, parentID, targetNode, tooltipArgs, unkDelegate, unk7, unk8);
    }

    #endregion

    internal override void Uninit()
    {
        GenerateItemTooltipHook.Disable();
        GenerateItemTooltipHook.Dispose();
        GenerateItemTooltipHook = null;
        
        GenerateActionTooltipHook.Disable();
        GenerateActionTooltipHook.Dispose();
        GenerateActionTooltipHook = null;
        
        ActionTooltipHook.Disable();
        ActionTooltipHook.Dispose();
        ActionTooltipHook = null;
        
        ActionHoveredHook.Disable();
        ActionHoveredHook.Dispose();
        ActionHoveredHook = null;
        
        TooltipShowHook.Disable();
        TooltipShowHook.Dispose();
        TooltipShowHook = null;
        
        Modifiers.Clear();
        TooltipModifications.Clear();
    }

}

#region 自定义类

public class TooltipModification
{
    public Guid              ID                 { get; set; } = Guid.NewGuid();
    public uint              ItemID             { get; set; }
    public uint              ActionID           { get; set; }
    public uint              StatuID            { get; set; }
    public TooltipItemType   ItemField          { get; set; }
    public TooltipActionType ActionField        { get; set; }
    public TooltipModifyMode Mode               { get; set; }
    public SeString          Text               { get; set; }
    public string            RegexPattern       { get; set; }
    public string            ReplacementPattern { get; set; }
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
    ControlsDisplay                       = 64,
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
    ClassJobAbbr,
}

#endregion
