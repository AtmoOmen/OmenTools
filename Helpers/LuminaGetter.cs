using Lumina.Excel;

namespace OmenTools.Helpers;

public static class LuminaGetter
{
    public static ExcelSheet<T> Get<T>() where T : struct, IExcelRow<T> => 
        DService.Instance().Data.GetExcelSheet<T>();

    public static SubrowExcelSheet<T> GetSub<T>() where T : struct, IExcelSubrow<T> => 
        DService.Instance().Data.GetSubrowExcelSheet<T>();

    public static bool TryGet<T>(out ExcelSheet<T> sheet) where T : struct, IExcelRow<T>
    {
        sheet = Get<T>();
        return true;
    }

    public static bool TryGetSub<T>(out SubrowExcelSheet<T> sheet) where T : struct, IExcelSubrow<T>
    {
        sheet = GetSub<T>();
        return true;
    }

    public static T? GetRow<T>(uint rowID) where T : struct, IExcelRow<T>
    {
        var sheet = TryGet<T>(out var excelSheet) ? excelSheet : null;
        return sheet?.GetRowOrDefault(rowID);
    }

    public static T? GetSubRow<T>(uint rowID, int index = 0) where T : struct, IExcelSubrow<T>
    {
        var sheet = TryGetSub<T>(out var excelSheet) ? excelSheet : null;
        return sheet?.GetRowOrDefault(rowID)?[index];
    }
    
    public static T GetRowOrDefault<T>(uint rowID) where T : struct, IExcelRow<T>
    {
        var sheet = TryGet<T>(out var excelSheet) ? excelSheet : null;
        return sheet!.GetRowOrDefault(rowID).GetValueOrDefault();
    }

    public static T GetSubRowOrDefault<T>(uint rowID, int index = 0) where T : struct, IExcelSubrow<T>
    {
        var sheet = TryGetSub<T>(out var excelSheet) ? excelSheet : null;
        return sheet!.GetRowOrDefault(rowID).GetValueOrDefault()[index];
    }

    public static bool TryGetRow<T>(uint rowID, out T item) where T : struct, IExcelRow<T>
    {
        var row = GetRow<T>(rowID);
        item = row.GetValueOrDefault();
        return row != null;
    }

    public static bool TryGetSubRow<T>(uint rowID, out T item, int index = 0) where T : struct, IExcelSubrow<T>
    {
        var row = GetSubRow<T>(rowID, index);
        item = row.GetValueOrDefault();
        return row != null;
    }
}
