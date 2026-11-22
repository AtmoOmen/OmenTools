using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class ZoneSelectCombo : LuminaComboBase<TerritoryType>
{
    public ZoneSelectCombo
    (
        string                     id,
        IEnumerable<TerritoryType> zones = null
    ) : base(id, null)
    {
        var data = zones ?? LuminaGetter.Get<TerritoryType>().Where(x => !string.IsNullOrEmpty(x.Name.ToString()));
        Searcher = new LuminaSearcher<TerritoryType>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.PlaceName.ValueNullable?.Name.ToString()              ?? string.Empty,
                x => x.PlaceNameZone.ValueNullable?.Name.ToString()          ?? string.Empty,
                x => x.PlaceNameRegion.ValueNullable?.Name.ToString()        ?? string.Empty,
                x => x.ContentFinderCondition.ValueNullable?.Name.ToString() ?? string.Empty
            ],
            resultLimit: 200
        );
    }

    public override uint          SelectedID  { get; set; }
    public override HashSet<uint> SelectedIDs { get; set; } = [];

    protected override string GetPreviewText
    (
        ComboSelectionMode mode
    )
    {
        if (mode == ComboSelectionMode.Radio)
        {
            return SelectedItem.RowId == 0 ?
                       string.Empty :
                       $"{SelectedItem.ExtractPlaceName()}";
        }

        return SelectedItems.Count == 0 ?
                   string.Empty :
                   $"[{SelectedItems.Count}] {SelectedItems.First().ExtractPlaceName()}...";
    }

    protected override int GetTableColumnCount() =>
        4;

    protected override void SetupColumns
    (
        ComboSelectionMode mode
    )
    {
        ImGui.TableSetupColumn("###Selected", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetFrameHeight());
        ImGui.TableSetupColumn("###Region",   ImGuiTableColumnFlags.WidthStretch, 20);
        ImGui.TableSetupColumn("###Zone",     ImGuiTableColumnFlags.WidthStretch, 50);
        ImGui.TableSetupColumn("###Duty",     ImGuiTableColumnFlags.WidthStretch, 50);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(2225));
    }

    protected override bool DrawDataColumns
    (
        TerritoryType      zone,
        ComboSelectionMode mode,
        bool               isSelected
    )
    {
        var zoneName    = zone.ExtractPlaceName();
        var regionName  = zone.PlaceNameRegion.ValueNullable?.Name.ToString()        ?? string.Empty;
        var contentName = zone.ContentFinderCondition.ValueNullable?.Name.ToString() ?? string.Empty;

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
        
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(contentName);

        return clicked;
    }
}
