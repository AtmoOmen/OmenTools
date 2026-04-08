using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Data;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class WorldSelectCombo : LuminaComboBase<World>
{
    public WorldSelectCombo(string id, IEnumerable<World> worlds = null) : base(id, null)
    {
        var data = worlds ?? Sheets.Worlds.Values;
        Searcher = new LuminaSearcher<World>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.DataCenter.Value.Name.ToString()
            ],
            x => x.OrderBy(w => w.UserType)
                  .ThenBy(w => w.DataCenter.RowId),
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
                       : $"[{SelectedItem.DataCenter.Value.Name.ToString()}] {SelectedItem.Name.ToString()}";
        }

        return SelectedItems.Count == 0
                   ? string.Empty
                   : $"[{SelectedItems.First().DataCenter.Value.Name.ToString()}] {SelectedItems.First().Name.ToString()}...";
    }

    protected override int GetTableColumnCount() =>
        3;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("World",      ImGuiTableColumnFlags.WidthStretch, 50);
        ImGui.TableSetupColumn("DataCenter", ImGuiTableColumnFlags.WidthStretch, 40);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(16222));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetLobbyText(802));
    }

    protected override bool DrawDataColumns(World world, ComboSelectionMode mode, bool isSelected)
    {
        var worldName = world.Name.ToString();

        ImGui.TableNextColumn();
        var clicked = ImGui.Selectable
        (
            $"{worldName}##World_{world.RowId}",
            isSelected,
            GetSelectableFlags(mode)
        );

        ImGuiOm.TooltipHover(worldName);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(world.DataCenter.Value.Name.ToString());
        return clicked;
    }
}
