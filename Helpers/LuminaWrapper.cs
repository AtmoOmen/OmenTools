using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;

namespace OmenTools.Helpers;

public static class LuminaWrapper
{
    public static string GetAddonText(uint rowID) => 
        LuminaGetter.TryGetRow<Addon>(rowID, out var item) ? item.Text.ExtractText() : string.Empty;
    
    public static string GetActionName(uint rowID) =>
        LuminaGetter.TryGetRow<Action>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetWorldName(uint rowID) =>
        LuminaGetter.TryGetRow<World>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;

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
}
