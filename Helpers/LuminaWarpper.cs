﻿using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;

namespace OmenTools.Helpers;

public static class LuminaWarpper
{
    public static string GetAddonText(uint rowID) => 
        LuminaGetter.TryGetRow<Addon>(rowID, out var item) ? item.Text.ExtractText() : string.Empty;
    
    public static string GetActionName(uint rowID) =>
        LuminaGetter.TryGetRow<Action>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
    
    public static string GetWorldName(uint rowID) =>
        LuminaGetter.TryGetRow<World>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;

    public static string GetItemName(uint rowID) =>
        LuminaGetter.TryGetRow<Item>(rowID, out var item) ? item.Name.ExtractText() : string.Empty;
}
