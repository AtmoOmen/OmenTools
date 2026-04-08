using System.Numerics;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class MountSelectCombo : LuminaComboBase<Mount>
{
    public MountSelectCombo(string id, IEnumerable<Mount> mounts = null) : base(id, null)
    {
        var data = mounts ?? LuminaGetter.Get<Mount>().Where(x => !string.IsNullOrEmpty(x.Singular.ToString()));
        Searcher = new LuminaSearcher<Mount>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Singular.ToString(),
                x => (x.ExtraSeats + 1).ToString()
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

    protected override Vector2 GetPopupSize() =>
        ScaledVector2(500f, 400f);

    protected override int GetTableColumnCount() =>
        2;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Mount", ImGuiTableColumnFlags.WidthStretch);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(6382));
    }

    protected override bool DrawDataColumns(Mount mount, ComboSelectionMode mode, bool isSelected)
    {
        ImGui.TableNextColumn();

        return DService.Instance().Texture.TryGetFromGameIcon(new(mount.Icon), out var texture)
                   ? ImGuiOm.SelectableImageWithText
                   (
                       texture.GetWrapOrEmpty().Handle,
                       new(ImGui.GetTextLineHeight()),
                       mount.Singular.ToString(),
                       mode == ComboSelectionMode.Checkbox && isSelected,
                       GetSelectableFlags(mode)
                   )
                   : ImGui.Selectable
                   (
                       $"{mount.Singular.ToString()}##Mount_{mount.RowId}",
                       mode == ComboSelectionMode.Checkbox && isSelected,
                       GetSelectableFlags(mode)
                   );
    }
}
