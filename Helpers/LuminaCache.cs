using Lumina.Excel;
using Microsoft.Extensions.Caching.Memory;

namespace OmenTools.Helpers;

public static class LuminaCache
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions());

    private static string GetSheetCacheKey<T>() where T : ExcelRow         => $"ExcelSheet_{typeof(T).FullName}";
    private static string GetRowCacheKey<T>(uint rowID) where T : ExcelRow => $"ExcelRow_{typeof(T).FullName}_{rowID}";

    public static ExcelSheet<T>? Get<T>() where T : ExcelRow => TryGetAndCacheSheet<T>(out var sheet) ? sheet : null;

    public static bool TryGet<T>(out ExcelSheet<T> sheet) where T : ExcelRow => TryGetAndCacheSheet(out sheet);

    public static T? GetRow<T>(uint rowID) where T : ExcelRow => TryGetRow<T>(rowID, out var item) ? item : null;

    public static bool TryGetRow<T>(uint rowID, out T? item) where T : ExcelRow
    {
        if (!TryGetAndCacheSheet<T>(out var sheet))
        {
            item = null;
            return false;
        }

        var rowCacheKey = GetRowCacheKey<T>(rowID);
        item = Cache.Get<T>(rowCacheKey);
        if (item != null)
        {
            return true;
        }

        item = sheet.GetRow(rowID);
        if (item != null)
        {
            Cache.Set(rowCacheKey, item);
        }

        return item != null;
    }

    private static bool TryGetAndCacheSheet<T>(out ExcelSheet<T>? sheet) where T : ExcelRow
    {
        var cacheKey = GetSheetCacheKey<T>();
        sheet = Cache.Get<ExcelSheet<T>>(cacheKey);

        if (sheet == null)
        {
            sheet = DService.Data.GetExcelSheet<T>();
            if (sheet != null) Cache.Set(cacheKey, sheet);
        }

        return sheet != null;
    }

    public static void ClearCache() => Cache.Clear();
}