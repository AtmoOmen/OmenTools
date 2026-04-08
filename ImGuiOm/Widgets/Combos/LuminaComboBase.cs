using System.Numerics;
using Lumina.Excel;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public abstract class LuminaComboBase<T>
(
    string            id,
    LuminaSearcher<T> searcher
)
    where T : struct, IExcelRow<T>
{
    protected enum ComboSelectionMode
    {
        Radio,
        Checkbox
    }

    protected string            SearchWord = string.Empty;
    protected LuminaSearcher<T> Searcher { get; init; } = searcher;
    protected string            ID       { get; init; } = id;

    public abstract uint          SelectedID  { get; set; }
    public abstract HashSet<uint> SelectedIDs { get; set; }

    public T SelectedItem =>
        LuminaGetter.GetRow<T>(SelectedID).GetValueOrDefault();

    public List<T> SelectedItems =>
        SelectedIDs.Select(x => LuminaGetter.GetRow<T>(x).GetValueOrDefault())
                   .Where(x => x.RowId > 0)
                   .ToList();

    public bool DrawRadio() =>
        DrawCore(ComboSelectionMode.Radio);

    public bool DrawCheckbox() =>
        DrawCore(ComboSelectionMode.Checkbox);

    protected abstract string GetPreviewText(ComboSelectionMode mode);

    protected abstract Vector2 GetPopupSize();

    protected abstract int GetTableColumnCount();

    protected virtual ImGuiTableFlags GetTableFlags() =>
        ImGuiTableFlags.Borders;

    protected virtual bool CanDrawItem(T item) =>
        true;

    protected abstract void SetupColumns(ComboSelectionMode mode);

    protected abstract void DrawHeaders();

    protected abstract bool DrawDataColumns(T item, ComboSelectionMode mode, bool isSelected);

    protected static ImGuiSelectableFlags GetSelectableFlags(ComboSelectionMode mode) =>
        mode == ComboSelectionMode.Radio
            ? ImGuiSelectableFlags.SpanAllColumns
            : ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups;

    private bool DrawCore(ComboSelectionMode mode)
    {
        using var drawID = ImRaii.PushId(ID);

        if (ImGui.BeginCombo("###Combo", GetPreviewText(mode), ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(GetPopupSize());
        using var popup = ImRaii.Popup($"###Popup_{ID}");
        if (!popup) return false;

        ImGui.SetNextItemWidth(-1f);
        if (ImGui.InputTextWithHint("###Search", LuminaWrapper.GetAddonText(8128), ref SearchWord, 128))
            Searcher.Search(SearchWord);

        ImGui.Separator();

        var       tableSize = new Vector2(ImGui.GetContentRegionAvail().X, 0f);
        using var table     = ImRaii.Table("###Table", GetTableColumnCount(), GetTableFlags(), tableSize);
        if (!table) return false;

        SetupColumns(mode);
        DrawHeaders();

        var visibleItems = BuildVisibleItems(mode);
        if (visibleItems.Count == 0) return false;

        var clipper     = new ImGuiListClipper();
        var selectState = false;

        clipper.Begin(visibleItems.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var item       = visibleItems[i];
                var isSelected = IsSelected(item, mode);

                using var rowID = ImRaii.PushId($"Row_{item.RowId}");

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                if (DrawSelector(mode, isSelected) || DrawDataColumns(item, mode, isSelected))
                    selectState |= ApplySelection(item.RowId, mode);
            }
        }

        clipper.End();
        return selectState;
    }

    private List<T> BuildVisibleItems(ComboSelectionMode mode)
    {
        var result = new List<T>();
        var seen   = new HashSet<uint>();

        if (mode == ComboSelectionMode.Radio)
        {
            AppendVisibleItem(SelectedItem, seen, result);

            foreach (var item in Searcher.SearchResult)
                AppendVisibleItem(item, seen, result);

            return result;
        }

        foreach (var item in SelectedItems)
            AppendVisibleItem(item, seen, result);

        foreach (var item in Searcher.SearchResult)
            AppendVisibleItem(item, seen, result);

        return result;
    }

    private void AppendVisibleItem(T item, HashSet<uint> seen, List<T> result)
    {
        if (item.RowId == 0 || !CanDrawItem(item) || !seen.Add(item.RowId)) return;
        result.Add(item);
    }

    private bool IsSelected(T item, ComboSelectionMode mode) =>
        mode             == ComboSelectionMode.Radio
            ? SelectedID == item.RowId
            : SelectedIDs.Contains(item.RowId);

    private static bool DrawSelector(ComboSelectionMode mode, bool isSelected)
    {
        if (mode == ComboSelectionMode.Radio)
            return ImGui.RadioButton(string.Empty, isSelected);

        return ImGui.Checkbox(string.Empty, ref isSelected);
    }

    private bool ApplySelection(uint rowID, ComboSelectionMode mode)
    {
        if (mode == ComboSelectionMode.Radio)
        {
            if (SelectedID == rowID) return false;

            SelectedID = rowID;
            return true;
        }

        if (!SelectedIDs.Remove(rowID))
            SelectedIDs.Add(rowID);

        return true;
    }
}
