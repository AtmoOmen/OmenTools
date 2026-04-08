using System.Numerics;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class JobSelectCombo : LuminaComboBase<ClassJob>
{
    public JobSelectCombo(string id, IEnumerable<ClassJob> jobs = null) : base(id, null)
    {
        var data = jobs ??
                   LuminaGetter.Get<ClassJob>()
                               .Where(x => !string.IsNullOrEmpty(x.Name.ToString()));
        Searcher = new LuminaSearcher<ClassJob>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.Abbreviation.ToString(),
                x => x.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty
            ],
            x => x.OrderBy(d => d.Role)
                  .ThenBy(d => d.DohDolJobIndex)
                  .ThenBy(d => d.ClassJobCategory.RowId),
            200
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

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Icon",     ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Job",      ImGuiTableColumnFlags.WidthStretch, 70);
        ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.WidthStretch, 30);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(7542));
    }

    protected override bool DrawDataColumns(ClassJob job, ComboSelectionMode mode, bool isSelected)
    {
        var iconID = 62100 + (job.RowId == 0 ? 44 : job.RowId);
        var icon   = ImageHelper.GetGameIcon(iconID);

        ImGui.TableNextColumn();
        ImGui.Image(icon.Handle, new(ImGui.GetTextLineHeightWithSpacing()));

        ImGui.TableNextColumn();
        var clicked = ImGui.Selectable(job.Name.ToString(), mode == ComboSelectionMode.Checkbox && isSelected, GetSelectableFlags(mode));

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(job.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty);
        return clicked;
    }
}
