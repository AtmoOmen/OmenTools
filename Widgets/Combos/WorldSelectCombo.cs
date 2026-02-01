using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class WorldSelectCombo : LuminaComboBase<World>
{
    public WorldSelectCombo(string id, IEnumerable<World> worlds = null) : base(id, null)
    {
        var data = worlds ?? PresetSheet.Worlds.Values;
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

    public override bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItem.RowId == 0
                          ? string.Empty
                          : $"[{SelectedItem.DataCenter.Value.Name.ToString()}] {SelectedItem.Name.ToString()}";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(500f, 400f));
        using var popup = ImRaii.Popup($"###Popup_{ID}");

        if (popup)
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("###Search", LuminaWrapper.GetAddonText(8128), ref SearchWord, 128))
                Searcher.Search(SearchWord);

            ImGui.Separator();

            var       tableSize = new Vector2(ImGui.GetContentRegionAvail().X, 0);
            using var table     = ImRaii.Table("###Table", 3, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("RadioButton", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("World",       ImGuiTableColumnFlags.WidthStretch, 50);
                ImGui.TableSetupColumn("DataCenter",  ImGuiTableColumnFlags.WidthStretch, 40);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(16222));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetLobbyText(802));

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                foreach (var world in Searcher.SearchResult)
                {
                    if (world.RowId == SelectedID) continue;
                    Render(world);
                }
            }
        }

        return selectState;

        void Render(World world)
        {
            var worldName = world.Name.ToString();
            var dcName    = world.DataCenter.Value.Name.ToString();

            using var id = ImRaii.PushId($"World_{world.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == world.RowId);

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{worldName}##World_{world.RowId}",
                    false,
                    ImGuiSelectableFlags.SpanAllColumns
                ))
            {
                SelectedID  = world.RowId;
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover(worldName);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(dcName);
        }
    }

    public override bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItems.Count == 0
                          ? string.Empty
                          : $"[{SelectedItems.First().DataCenter.Value.Name.ToString()}] {SelectedItems.First().Name.ToString()}...";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(500f, 400f));
        using var popup = ImRaii.Popup($"###Popup_{ID}");

        if (popup)
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("###Search", LuminaWrapper.GetAddonText(8128), ref SearchWord, 128))
                Searcher.Search(SearchWord);

            ImGui.Separator();

            var       tableSize = new Vector2(ImGui.GetContentRegionAvail().X, 0);
            using var table     = ImRaii.Table("###Table", 3, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("Checkbox",   ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("World",      ImGuiTableColumnFlags.WidthStretch, 50);
                ImGui.TableSetupColumn("DataCenter", ImGuiTableColumnFlags.WidthStretch, 40);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(16222));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetLobbyText(802));

                foreach (var world in SelectedItems)
                    Render(world);

                foreach (var world in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(world.RowId)) continue;
                    Render(world);
                }
            }
        }

        return selectState;

        void Render(World world)
        {
            var worldName = world.Name.ToString();
            var dcName    = world.DataCenter.Value.Name.ToString();

            using var id = ImRaii.PushId($"World_{world.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedIDs.Contains(world.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(world.RowId))
                    SelectedIDs.Add(world.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{worldName}##World_{world.RowId}",
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                ))
            {
                if (!SelectedIDs.Remove(world.RowId))
                    SelectedIDs.Add(world.RowId);
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover(worldName);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(dcName);
        }
    }
}
