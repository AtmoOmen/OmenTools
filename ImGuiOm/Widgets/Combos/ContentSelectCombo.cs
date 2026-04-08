using System.Numerics;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Data;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class ContentSelectCombo : LuminaComboBase<ContentFinderCondition>
{
    public ContentSelectCombo(string id, IEnumerable<ContentFinderCondition> contents = null) : base(id, null)
    {
        var data = contents ?? Sheets.Contents.Values;
        Searcher = new LuminaSearcher<ContentFinderCondition>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.TerritoryType.ValueNullable?.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty
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
        ScaledVector2(600f, 400f);

    protected override int GetTableColumnCount() =>
        5;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
        (
            mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox",
            ImGuiTableColumnFlags.WidthFixed,
            mode == ComboSelectionMode.Radio ? 20f * GlobalUIScale : ImGui.GetTextLineHeightWithSpacing()
        );
        ImGui.TableSetupColumn("Icon",      ImGuiTableColumnFlags.WidthFixed,   20f * GlobalUIScale);
        ImGui.TableSetupColumn("Level",     ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize(LuminaWrapper.GetAddonText(335)).X);
        ImGui.TableSetupColumn("DutyName",  ImGuiTableColumnFlags.WidthStretch, 40);
        ImGui.TableSetupColumn("PlaceName", ImGuiTableColumnFlags.WidthStretch, 40);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(14098));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));
    }

    protected override bool DrawDataColumns(ContentFinderCondition content, ComboSelectionMode mode, bool isSelected)
    {
        var contentName = content.Name.ToString();
        var placeName   = content.TerritoryType.ValueNullable?.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty;

        ImGui.TableNextColumn();
        if (DService.Instance().Texture.TryGetFromGameIcon(new(content.ContentType.ValueNullable?.Icon ?? 0), out var icon))
            ImGui.Image(icon.GetWrapOrEmpty().Handle, ScaledVector2(20f));

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(content.ClassJobLevelRequired.ToString());

        ImGui.TableNextColumn();
        var clicked = ImGui.Selectable
        (
            $"{contentName}##Content_{content.RowId}",
            mode == ComboSelectionMode.Checkbox && isSelected,
            GetSelectableFlags(mode)
        );

        if (DService.Instance().Texture.TryGetFromGameIcon(new(content.Image), out var image) && ImGui.IsItemHovered())
        {
            using (ImRaii.Tooltip())
                ImGui.Image(image.GetWrapOrEmpty().Handle, image.GetWrapOrEmpty().Size / 2);
        }

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(placeName);
        return clicked;
    }
}
