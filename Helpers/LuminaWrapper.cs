using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Helpers;

public static class LuminaWrapper
{
    public static string GetAddonText(uint rowID) => 
        LuminaGetter.TryGetRow<Addon>(rowID, out var item) ? item.Text.ToString() : string.Empty;
    
    public static SeString GetAddonTextSeString(uint rowID) => 
        LuminaGetter.TryGetRow<Addon>(rowID, out var item) ? item.Text.ToDalamudString() : SeString.Empty;
    
    public static string GetDynamicEventName(uint rowID) => 
        LuminaGetter.TryGetRow<DynamicEvent>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetDescriptionString(uint rowID) => 
        LuminaGetter.TryGetRow<DescriptionString>(rowID, out var item) ? item.Text.ToString() : string.Empty;

    public static string GetLogMessageText(uint rowID) =>
        LuminaGetter.TryGetRow<LogMessage>(rowID, out var item) ? item.Text.ToString() : string.Empty;
    
    public static string GetLobbyText(uint rowID) => 
        LuminaGetter.TryGetRow<Lobby>(rowID, out var item) ? item.Text.ToString() : string.Empty;
    
    public static string GetActionName(uint rowID)
    {
        if (rowID > 10_0000)
            return LuminaGetter.TryGetRow<CraftAction>(rowID, out var item) ? item.Name.ToString() : string.Empty;
        else
            return LuminaGetter.TryGetRow<Action>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    }
    
    public static uint GetActionIconID(uint rowID)
    {
        if (rowID > 10_0000)
            return LuminaGetter.TryGetRow<CraftAction>(rowID, out var item) ? item.Icon : 0U;
        else
            return LuminaGetter.TryGetRow<Action>(rowID, out var item) ? item.Icon : 0U;
    }
    
    public static string GetGeneralActionName(uint rowID) => 
        LuminaGetter.TryGetRow<GeneralAction>(rowID, out var item) ? item.Name.ToString() : string.Empty;

    public static string GetChocoboRaceAbilityName(uint rowID) => 
        LuminaGetter.TryGetRow<ChocoboRaceAbility>(rowID, out var item) ? item.Name.ToString() : string.Empty;

    public static string GetWorldName(uint rowID) =>
        LuminaGetter.TryGetRow<World>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetWorldDCName(uint rowID) =>
        LuminaGetter.TryGetRow<World>(rowID, out var item) ? item.DataCenter.Value.Name.ToString() : string.Empty;
    
    public static string GetDataCenterName(uint rowID) =>
        LuminaGetter.TryGetRow<WorldDCGroupType>(rowID, out var item) ? item.Name.ToString() : string.Empty;

    public static string GetItemName(uint rowID) =>
        LuminaGetter.TryGetRow<Item>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static uint GetItemIconID(uint rowID) =>
        LuminaGetter.TryGetRow<Item>(rowID, out var item) ? item.Icon : 0U;
    
    public static string GetStatusName(uint rowID) =>
        LuminaGetter.TryGetRow<Status>(rowID, out var item) ? item.Name.ToString() : string.Empty;

    public static string GetZonePlaceName(uint rowID) =>
        LuminaGetter.TryGetRow<TerritoryType>(rowID, out var item) ? 
            item.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty : 
            string.Empty;

    public static string GetContentName(uint rowID) =>
        LuminaGetter.TryGetRow<ContentFinderCondition>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetContentRouletteName(uint rowID) =>
        LuminaGetter.TryGetRow<ContentRoulette>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetPlaceName(uint rowID) =>
        LuminaGetter.TryGetRow<PlaceName>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetGatheringPointName(uint rowID) =>
        LuminaGetter.TryGetRow<GatheringPointName>(rowID, out var item) ? item.Singular.ToString() : string.Empty;
    
    public static string GetBNPCName(uint rowID) =>
        LuminaGetter.TryGetRow<BNpcName>(rowID, out var item) ? item.Singular.ToString() : string.Empty;
    
    public static string GetENPCName(uint rowID) =>
        LuminaGetter.TryGetRow<ENpcResident>(rowID, out var item) ? item.Singular.ToString() : string.Empty;
    
    public static string GetENPCTitle(uint rowID) =>
        LuminaGetter.TryGetRow<ENpcResident>(rowID, out var item) ? item.Title.ToString() : string.Empty;
    
    public static string GetEObjName(uint rowID) =>
        LuminaGetter.TryGetRow<EObjName>(rowID, out var item) ? item.Singular.ToString() : string.Empty;
    
    public static string GetJobName(uint rowID) =>
        LuminaGetter.TryGetRow<ClassJob>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetAchievementKindName(uint rowID) =>
        LuminaGetter.TryGetRow<AchievementKind>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetFCChestName(uint rowID) => 
        LuminaGetter.TryGetRow<FCChestName>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetMKDSupportJobName(uint rowID) => 
        LuminaGetter.TryGetRow<MKDSupportJob>(rowID, out var item) ? item.Name.ToString() : string.Empty;
    
    public static string GetMKDSupportJobDescription(uint rowID) => 
        LuminaGetter.TryGetRow<MKDSupportJob>(rowID, out var item) ? item.Description.ToString() : string.Empty;
    
    public static string GetMKDTraitName(uint rowID) => 
        LuminaGetter.TryGetRow<MKDTrait>(rowID, out var item) ? item.Unknown0.ToString() : string.Empty;
    
    public static string GetWeatherName(uint rowID) => 
        LuminaGetter.TryGetRow<Weather>(rowID, out var item) ? item.Name.ToString() : string.Empty;
}
