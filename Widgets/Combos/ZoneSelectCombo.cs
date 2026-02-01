using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class ZoneSelectCombo : LuminaComboBase<TerritoryType>
{
    public ZoneSelectCombo(string id, IEnumerable<TerritoryType> zones = null) : base(id, null)
    {
        var data = zones ?? LuminaGetter.Get<TerritoryType>().Where(x => !string.IsNullOrEmpty(x.Name.ToString()));
        Searcher = new LuminaSearcher<TerritoryType>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.PlaceName.ValueNullable?.Name.ToString()       ?? string.Empty,
                x => x.PlaceNameZone.ValueNullable?.Name.ToString()   ?? string.Empty,
                x => x.PlaceNameRegion.ValueNullable?.Name.ToString() ?? string.Empty
            ],
            resultLimit: 200
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
                          : $"{SelectedItem.ExtractPlaceName()} ({SelectedItem.RowId})";
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
                ImGui.TableSetupColumn("Region",      ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("Zone",        ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                foreach (var zone in Searcher.SearchResult)
                {
                    if (zone.RowId == SelectedID) continue;
                    Render(zone);
                }
            }
        }

        return selectState;

        void Render(TerritoryType zone)
        {
            var zoneName   = zone.ExtractPlaceName();
            var regionName = zone.PlaceNameRegion.ValueNullable?.Name.ToString() ?? string.Empty;
            if (zone.ContentFinderCondition.RowId > 0                                &&
                zone.ContentFinderCondition.Value.Name.ToString() is var contentName &&
                !string.IsNullOrEmpty(contentName))
                zoneName += $" ({contentName})";

            using var id = ImRaii.PushId($"Zone_{zone.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == zone.RowId);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(regionName);

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{zoneName}##Zone_{zone.RowId}",
                    false,
                    ImGuiSelectableFlags.SpanAllColumns
                ))
            {
                SelectedID  = zone.RowId;
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover(zoneName);
        }
    }

    public override bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItems.Count == 0
                          ? string.Empty
                          : $"[{SelectedItems.Count}] {SelectedItems.First().ExtractPlaceName()} ({SelectedItems.First().RowId})...";
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
                ImGui.TableSetupColumn("Checkbox", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Region",   ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("Zone",     ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));

                foreach (var zone in SelectedItems)
                    Render(zone);

                foreach (var zone in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(zone.RowId)) continue;
                    Render(zone);
                }
            }
        }

        return selectState;

        void Render(TerritoryType zone)
        {
            var zoneName   = zone.ExtractPlaceName();
            var regionName = zone.PlaceNameRegion.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Zone_{zone.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedIDs.Contains(zone.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(zone.RowId))
                    SelectedIDs.Add(zone.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(regionName);

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{zoneName}##Zone_{zone.RowId}",
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                ))
            {
                if (!SelectedIDs.Remove(zone.RowId))
                    SelectedIDs.Add(zone.RowId);
                selectState = true;
            }
        }
    }
}
