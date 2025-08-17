using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Helpers;

public static class LuminaWrapper
{
    public static string GetAddonText(uint rowID) => 
        LuminaGetter.TryGetRow<Addon>(rowID, out var item) ? item.Text.ExtractText() : string.Empty;
    
    public static string GetDynamicEventName(uint rowID) => 
        LuminaGetter.TryGetRow<DynamicEvent>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetDescriptionString(uint rowID) => 
        LuminaGetter.TryGetRow<DescriptionString>(rowID, out var item) ? item.Text.ExtractText() : string.Empty;

    public static string GetLogMessageText(uint rowID) =>
        LuminaGetter.TryGetRow<LogMessage>(rowID, out var item) ? item.Text.ExtractText() : string.Empty;
    
    public static string GetLobbyText(uint rowID) => 
        LuminaGetter.TryGetRow<Lobby>(rowID, out var item) ? item.Text.ExtractText() : string.Empty;
    
    public static string GetActionName(uint rowID)
    {
        if (rowID > 10_0000)
            return LuminaGetter.TryGetRow<CraftAction>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
        else
            return LuminaGetter.TryGetRow<Action>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    }

    public static string GetWorldName(uint rowID) =>
        LuminaGetter.TryGetRow<World>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetWorldDCName(uint rowID) =>
        LuminaGetter.TryGetRow<World>(rowID, out var item) ? item.DataCenter.Value.Name.ExtractText() : string.Empty;
    
    public static string GetDataCenterName(uint rowID) =>
        LuminaGetter.TryGetRow<WorldDCGroupType>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;

    public static string GetItemName(uint rowID) =>
        LuminaGetter.TryGetRow<Item>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetStatusName(uint rowID) =>
        LuminaGetter.TryGetRow<Status>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;

    public static string GetZonePlaceName(uint rowID) =>
        LuminaGetter.TryGetRow<TerritoryType>(rowID, out var item) ? 
            item.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty : 
            string.Empty;

    public static string GetContentName(uint rowID) =>
        LuminaGetter.TryGetRow<ContentFinderCondition>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetPlaceName(uint rowID) =>
        LuminaGetter.TryGetRow<PlaceName>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetGatheringPointName(uint rowID) =>
        LuminaGetter.TryGetRow<GatheringPointName>(rowID, out var item) ? item.Singular.ExtractText() : string.Empty;
    
    public static string GetBNpcName(uint rowID) =>
        LuminaGetter.TryGetRow<BNpcName>(rowID, out var item) ? item.Singular.ExtractText() : string.Empty;
    
    public static string GetENpcName(uint rowID) =>
        LuminaGetter.TryGetRow<ENpcResident>(rowID, out var item) ? item.Singular.ExtractText() : string.Empty;
    
    public static string GetENpcTitle(uint rowID) =>
        LuminaGetter.TryGetRow<ENpcResident>(rowID, out var item) ? item.Title.ExtractText() : string.Empty;
    
    public static string GetEObjName(uint rowID) =>
        LuminaGetter.TryGetRow<EObjName>(rowID, out var item) ? item.Singular.ExtractText() : string.Empty;
    
    public static string GetJobName(uint rowID) =>
        LuminaGetter.TryGetRow<ClassJob>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetAchievementKindName(uint rowID) =>
        LuminaGetter.TryGetRow<AchievementKind>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetFCChestName(uint rowID) => 
        LuminaGetter.TryGetRow<FCChestName>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetMKDSupportJobName(uint rowID) => 
        LuminaGetter.TryGetRow<MKDSupportJob>(rowID, out var item) ? item.Unknown0.ExtractText() : string.Empty;
}
