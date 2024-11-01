using Lumina.Excel;

namespace OmenTools.Helpers;

public static class LuminaCache
{
    public static ExcelSheet<T>? Get<T>() where T : ExcelRow 
        => DService.Data.GetExcelSheet<T>();

    public static bool TryGet<T>(out ExcelSheet<T>? sheet) where T : ExcelRow
    {
        sheet = DService.Data.GetExcelSheet<T>();
        return sheet != null;
    }

    public static T? GetRow<T>(uint rowID) where T : ExcelRow 
        => DService.Data.GetExcelSheet<T>()?.GetRow(rowID);

    public static bool TryGetRow<T>(uint rowID, out T? item) where T : ExcelRow
    {
        item = DService.Data.GetExcelSheet<T>()?.GetRow(rowID);
        return item != null;
    }
}