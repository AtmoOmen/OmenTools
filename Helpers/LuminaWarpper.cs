using Lumina.Excel.Sheets;

namespace OmenTools.Helpers;

public static class LuminaWarpper
{
    public static string GetAddonText(uint rowID) => 
        LuminaGetter.TryGetRow<Addon>(rowID, out var item) ? item.Text.ExtractText() : string.Empty;
}
