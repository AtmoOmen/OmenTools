using Lumina.Excel;

namespace OmenTools.Widgets;

public abstract class LuminaComboBase<T>
(
    string            id,
    LuminaSearcher<T> searcher
)
    where T : struct, IExcelRow<T>
{
    protected LuminaSearcher<T> Searcher { get; init; } = searcher;
    protected string            ID       { get; init; } = id;

    protected string SearchWord = string.Empty;

    public abstract uint          SelectedID  { get; set; }
    public abstract HashSet<uint> SelectedIDs { get; set; }

    public T SelectedItem =>
        LuminaGetter.GetRow<T>(SelectedID).GetValueOrDefault();

    public List<T> SelectedItems =>
        SelectedIDs.Select(x => LuminaGetter.GetRow<T>(x).GetValueOrDefault())
                   .Where(x => x.RowId > 0)
                   .ToList();

    public abstract bool DrawRadio();

    public abstract bool DrawCheckbox();
}
