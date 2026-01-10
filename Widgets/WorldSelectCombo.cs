using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class WorldSelectCombo
{
    public LuminaSearcher<World> Searcher { get; init; }
    public string ID { get; init; }

    public World SelectedWorld =>
        LuminaGetter.GetRow<World>(SelectedWorldID).GetValueOrDefault();

    public List<World> SelectedWorlds =>
        SelectedWorldIDs.Select(x => LuminaGetter.GetRow<World>(x).GetValueOrDefault())
                        .Where(x => x.RowId > 0)
                        .ToList();

    public uint          SelectedWorldID  { get; set; }
    public HashSet<uint> SelectedWorldIDs { get; set; } = [];

    public string SearchWord = string.Empty;

    public WorldSelectCombo(string id, IEnumerable<World> worlds = null)
    {
        ID = id;

        var data = worlds ?? PresetSheet.Worlds.Values;
        Searcher = new LuminaSearcher<World>(data,
                                             [
                                                 x => x.RowId.ToString(),
                                                 x => x.Name.ToString(),
                                                 x => x.DataCenter.Value.Name.ToString()
                                             ],
                                             x => x.OrderBy(w => w.UserType)
                                                   .ThenBy(w => w.DataCenter.RowId),
                                             200);
    }

    public bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedWorld.RowId == 0
                          ? string.Empty
                          : $"[{SelectedWorld.DataCenter.Value.Name.ToString()}] {SelectedWorld.Name.ToString()}";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup("###Popup");

        ImGui.SetNextWindowSize(ScaledVector2(500f, 400f));
        using var popup = ImRaii.Popup("###Popup");
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

                if (SelectedWorld is { RowId: > 0 })
                    Render(SelectedWorld);

                foreach (var world in Searcher.SearchResult)
                {
                    if (world.RowId == SelectedWorldID) continue;
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
            ImGui.RadioButton(string.Empty, SelectedWorld.RowId == world.RowId);

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{worldName}##World_{world.RowId}", false,
                                 ImGuiSelectableFlags.SpanAllColumns))
            {
                SelectedWorldID = world.RowId;
                selectState     = true;
            }
            ImGuiOm.ImGuiOm.TooltipHover(worldName);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(dcName);
        }
    }

    public bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedWorlds.Count == 0
                          ? string.Empty
                          : $"[{SelectedWorlds.First().DataCenter.Value.Name.ToString()}] {SelectedWorlds.First().Name.ToString()}...";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup("###Popup");

        ImGui.SetNextWindowSize(ScaledVector2(500f, 400f));
        using var popup = ImRaii.Popup("###Popup");
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

                foreach (var world in SelectedWorlds)
                    Render(world);

                foreach (var world in Searcher.SearchResult)
                {
                    if (SelectedWorldIDs.Contains(world.RowId)) continue;
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
            var isSelected = SelectedWorldIDs.Contains(world.RowId);
            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedWorldIDs.Remove(world.RowId))
                    SelectedWorldIDs.Add(world.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{worldName}##World_{world.RowId}", isSelected,
                                 ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups))
            {
                if (!SelectedWorldIDs.Remove(world.RowId))
                    SelectedWorldIDs.Add(world.RowId);
                selectState = true;
            }
            ImGuiOm.ImGuiOm.TooltipHover(worldName);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(dcName);
        }
    }
}
