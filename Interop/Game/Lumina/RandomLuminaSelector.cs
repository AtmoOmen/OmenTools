using Lumina.Excel;

namespace OmenTools.Interop.Game.Lumina;

public class RandomLuminaSelector<T, TRet> where T : struct, IExcelRow<T>
{
    private readonly Func<T, TRet> getterFunc;
        
    private readonly List<uint> validRowIDs = [];
        
    public RandomLuminaSelector(T[] items, Func<T, TRet> getter)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(getter);
            
        foreach (var row in items)
            validRowIDs.Add(row.RowId);

        getterFunc = getter;
    }
        
    public TRet Get() => 
        getterFunc(LuminaGetter.GetRowOrDefault<T>((uint)Random.Shared.Next(validRowIDs.Count - 1)));
}
