using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class CompanionCombo : LuminaComboBase<Companion>
{
    public CompanionCombo(string id, IEnumerable<Companion> companions = null) : base(id, null)
    {
        var data = companions ?? LuminaGetter.Get<Companion>().Where(x => !string.IsNullOrEmpty(x.Singular.ToString()));
        Searcher = new LuminaSearcher<Companion>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Singular.ToString(),
                x => GetDescription(x)
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
                       : $"{SelectedItem.Singular.ToString()} ({SelectedItem.RowId})";
        }

        return SelectedItems.Count == 0
                   ? string.Empty
                   : $"[{SelectedItems.Count}] {SelectedItems.First().Singular.ToString()} ({SelectedItems.First().RowId})...";
    }

    protected override int GetTableColumnCount() =>
        2;

    protected override bool CanDrawItem(Companion item) =>
        DService.Instance().Texture.TryGetFromGameIcon(new(item.Icon), out _);

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Companion", ImGuiTableColumnFlags.WidthStretch, 50);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(6382));
    }

    protected override bool DrawDataColumns(Companion companion, ComboSelectionMode mode, bool isSelected)
    {
        var name        = companion.Singular.ToString();
        var description = GetDescription(companion);
        var displayText = $"{name} ({companion.RowId})";

        ImGui.TableNextColumn();

        var clicked = DService.Instance().Texture.TryGetFromGameIcon(new(companion.Icon), out var texture)
                          ? ImGuiOm.SelectableImageWithText
                          (
                              texture.GetWrapOrEmpty().Handle,
                              new(ImGui.GetTextLineHeightWithSpacing()),
                              displayText,
                              mode == ComboSelectionMode.Checkbox && isSelected,
                              GetSelectableFlags(mode)
                          )
                          : ImGui.Selectable
                          (
                              $"{displayText}##Companion_{companion.RowId}_{name}",
                              mode == ComboSelectionMode.Checkbox && isSelected,
                              GetSelectableFlags(mode)
                          );

        if (!string.IsNullOrWhiteSpace(description))
            ImGuiOm.TooltipHover(description);

        return clicked;
    }

    private static string GetDescription(Companion companion) =>
        LuminaGetter.TryGetRow<CompanionTransient>(companion.RowId, out var transient)
            ? transient.Description.ToString() ?? string.Empty
            : string.Empty;
}
