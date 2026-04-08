using System.Numerics;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

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

    protected override string GetPreviewText(ComboSelectionMode mode)
    {
        if (mode == ComboSelectionMode.Radio)
        {
            return SelectedItem.RowId == 0
                       ? string.Empty
                       : $"{SelectedItem.ExtractPlaceName()} ({SelectedItem.RowId})";
        }

        return SelectedItems.Count == 0
                   ? string.Empty
                   : $"[{SelectedItems.Count}] {SelectedItems.First().ExtractPlaceName()} ({SelectedItems.First().RowId})...";
    }

    protected override Vector2 GetPopupSize() =>
        ScaledVector2(500f, 400f);

    protected override int GetTableColumnCount() =>
        3;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Region", ImGuiTableColumnFlags.WidthStretch, 40);
        ImGui.TableSetupColumn("Zone",   ImGuiTableColumnFlags.WidthStretch, 50);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));
    }

    protected override bool DrawDataColumns(TerritoryType zone, ComboSelectionMode mode, bool isSelected)
    {
        var zoneName   = zone.ExtractPlaceName();
        var regionName = zone.PlaceNameRegion.ValueNullable?.Name.ToString() ?? string.Empty;

        if (mode                              == ComboSelectionMode.Radio        &&
            zone.ContentFinderCondition.RowId > 0                                &&
            zone.ContentFinderCondition.Value.Name.ToString() is var contentName &&
            !string.IsNullOrEmpty(contentName)) zoneName += $" ({contentName})";

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(regionName);

        ImGui.TableNextColumn();
        var clicked = ImGui.Selectable
        (
            $"{zoneName}##Zone_{zone.RowId}",
            mode == ComboSelectionMode.Checkbox && isSelected,
            GetSelectableFlags(mode)
        );

        if (mode == ComboSelectionMode.Radio)
            ImGuiOm.TooltipHover(zoneName);

        return clicked;
    }
}
