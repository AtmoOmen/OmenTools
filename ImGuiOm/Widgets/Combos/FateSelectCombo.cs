using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class FateSelectCombo : LuminaComboBase<Fate>
{
    public FateSelectCombo(string id, IEnumerable<Fate> fates = null) : base(id, null)
    {
        var data = fates ?? LuminaGetter.Get<Fate>().Where(x => !string.IsNullOrEmpty(x.Name.ToString())).OrderBy(x => x.ClassJobLevel);
        Searcher = new LuminaSearcher<Fate>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.Description.ToString(),
                x => x.Objective.ToString()
            ],
            resultLimit: 200
        );
    }

    public override uint          SelectedID  { get; set; }
    public override HashSet<uint> SelectedIDs { get; set; } = [];

    protected override string GetPreviewText(ComboSelectionMode mode)
    {
        if (mode == ComboSelectionMode.Radio)
        {
            return SelectedItem.RowId == 0
                       ? string.Empty
                       : $"{SelectedItem.Name.ToString()} ({SelectedItem.RowId})";
        }

        return SelectedItems.Count == 0
                   ? string.Empty
                   : $"[{SelectedItems.Count}] {SelectedItems.First().Name.ToString()} ({SelectedItems.First().RowId})...";
    }

    protected override int GetTableColumnCount() =>
        3;

    protected override bool CanDrawItem(Fate item) =>
        DService.Instance().Texture.TryGetFromGameIcon(new(item.Icon), out _);

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize("1234").X);
        ImGui.TableSetupColumn("Fate",  ImGuiTableColumnFlags.WidthStretch, 40);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));
    }

    protected override bool DrawDataColumns(Fate fate, ComboSelectionMode mode, bool isSelected)
    {
        DService.Instance().Texture.TryGetFromGameIcon(new(fate.Icon), out var texture);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(fate.ClassJobLevel.ToString());

        ImGui.TableNextColumn();
        var clicked = ImGuiOm.SelectableImageWithText
        (
            texture.GetWrapOrEmpty().Handle,
            new(ImGui.GetTextLineHeight()),
            fate.Name.ToString(),
            mode == ComboSelectionMode.Checkbox && isSelected,
            GetSelectableFlags(mode)
        );

        ImGuiOm.TooltipHover(fate.Description.ToString());
        return clicked;
    }
}
