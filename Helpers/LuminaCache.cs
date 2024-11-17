using Lumina.Excel;

namespace OmenTools.Helpers;

public static class LuminaCache
{
    public static ExcelSheet<T>? Get<T>() where T : struct, IExcelRow<T>
    {
        try
        {
            return DService.Data.GetExcelSheet<T>();
        }
        catch
        {
            return null;
        }
    }

    public static bool TryGet<T>(out ExcelSheet<T>? sheet) where T : struct, IExcelRow<T>
    {
        sheet = Get<T>();
        return sheet != null;
    }

    public static T? GetRow<T>(uint rowID) where T : struct, IExcelRow<T>
    {
        var sheet = TryGet<T>(out var excelSheet) ? excelSheet : null;
        if (sheet == null) return null;

        try
        {
            return sheet.GetRow(rowID);
        }
        catch
        {
            return null;
        }
    }

    public static bool TryGetRow<T>(uint rowID, out T? item) where T : struct, IExcelRow<T>
    {
        item = GetRow<T>(rowID);
        return item.HasValue;
    }
}