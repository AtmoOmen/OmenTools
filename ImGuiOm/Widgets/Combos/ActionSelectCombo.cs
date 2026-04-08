using System.Numerics;
using OmenTools.Info.Game.Data;
using OmenTools.Interop.Game.Lumina;
using Action = Lumina.Excel.Sheets.Action;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class ActionSelectCombo : LuminaComboBase<Action>
{
    public ActionSelectCombo(string id, IEnumerable<Action> actions = null) : base(id, null)
    {
        var data = actions ?? Sheets.PlayerActions.Values;
        Searcher = new LuminaSearcher<Action>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.ClassJob.ValueNullable?.Name.ToString() ?? string.Empty
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
                       : $"[{SelectedItem.ClassJobCategory.ValueNullable?.Name.ToString()}] {SelectedItem.Name.ToString()} ({SelectedItem.RowId})";
        }

        return SelectedItems.Count == 0
                   ? string.Empty
                   : $"[{SelectedItems.First().ClassJobCategory.ValueNullable?.Name.ToString()}] " +
                     $"{SelectedItems.First().Name.ToString()} "                                   +
                     $"({SelectedItems.First().RowId})...";
    }

    protected override Vector2 GetPopupSize() =>
        ScaledVector2(500f, 400f);

    protected override int GetTableColumnCount() =>
        4;

    protected override bool CanDrawItem(Action item) =>
        DService.Instance().Texture.TryGetFromGameIcon(new(item.Icon), out _);

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Job",    ImGuiTableColumnFlags.WidthStretch, 20);
        ImGui.TableSetupColumn("Level",  ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize("1234").X);
        ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch, 50);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));
    }

    protected override bool DrawDataColumns(Action action, ComboSelectionMode mode, bool isSelected)
    {
        DService.Instance().Texture.TryGetFromGameIcon(new(action.Icon), out var texture);

        var actionName = action.Name.ToString();
        var jobName    = action.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty;

        ImGui.TableNextColumn();
        var clicked = ImGui.Selectable
        (
            $"{jobName}##Action_{action.RowId}_{action.Name.ToString()}",
            mode == ComboSelectionMode.Checkbox && isSelected,
            GetSelectableFlags(mode)
        );

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(action.ClassJobLevel.ToString());

        ImGui.TableNextColumn();
        ImGuiOm.TextImage($"{actionName} ({action.RowId})", texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
        return clicked;
    }
}
