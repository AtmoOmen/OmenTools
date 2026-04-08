using System.Numerics;
using OmenTools.Info.Game.Data;
using OmenTools.Interop.Game.Lumina;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class StatusSelectCombo : LuminaComboBase<Status>
{
    public StatusSelectCombo(string id, IEnumerable<Status> statuses = null) : base(id, null)
    {
        var data = statuses ?? Sheets.Statuses.Values;
        Searcher = new LuminaSearcher<Status>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.Description.ToString() ?? string.Empty
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

    protected override Vector2 GetPopupSize() =>
        ScaledVector2(500f, 400f);

    protected override int GetTableColumnCount() =>
        3;

    protected override bool CanDrawItem(Status item) =>
        DService.Instance().Texture.TryGetFromGameIcon(new(item.Icon), out _);

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Job",    ImGuiTableColumnFlags.WidthStretch, 40);
        ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.WidthStretch, 50);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));
    }

    protected override bool DrawDataColumns(Status status, ComboSelectionMode mode, bool isSelected)
    {
        DService.Instance().Texture.TryGetFromGameIcon(new(status.Icon), out var texture);

        ImGui.TableNextColumn();
        var clicked = ImGui.Selectable
        (
            $"{status.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty}##Status_{status.RowId}_{status.Name.ToString()}",
            mode == ComboSelectionMode.Checkbox && isSelected,
            GetSelectableFlags(mode)
        );

        ImGuiOm.TooltipHover(status.Description.ToString());

        ImGui.TableNextColumn();
        ImGuiOm.TextImage(status.Name.ToString(), texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
        return clicked;
    }
}
