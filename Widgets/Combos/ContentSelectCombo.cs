using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class ContentSelectCombo : LuminaComboBase<ContentFinderCondition>
{
    public ContentSelectCombo(string id, IEnumerable<ContentFinderCondition> contents = null) : base(id, null)
    {
        var data = contents ?? PresetSheet.Contents.Values;
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

    public override bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItem.RowId == 0
                          ? string.Empty
                          : $"{SelectedItem.Name.ToString()} ({SelectedItem.RowId})";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
        using var popup = ImRaii.Popup($"###Popup_{ID}");

        if (popup)
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("###Search", LuminaWrapper.GetAddonText(8128), ref SearchWord, 128))
                Searcher.Search(SearchWord);

            ImGui.Separator();

            var       tableSize = new Vector2(ImGui.GetContentRegionAvail().X, 0);
            using var table     = ImRaii.Table("###Table", 5, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("RadioButton", ImGuiTableColumnFlags.WidthFixed,   20f * GlobalFontScale);
                ImGui.TableSetupColumn("Icon",        ImGuiTableColumnFlags.WidthFixed,   20f * GlobalFontScale);
                ImGui.TableSetupColumn("Level",       ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize(LuminaWrapper.GetAddonText(335)).X);
                ImGui.TableSetupColumn("DutyName",    ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("PlaceName",   ImGuiTableColumnFlags.WidthStretch, 40);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(14098));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                foreach (var content in Searcher.SearchResult)
                {
                    if (content.RowId == SelectedID) continue;
                    Render(content);
                }
            }
        }

        return selectState;

        void Render(ContentFinderCondition content)
        {
            var contentName = content.Name.ToString();
            var placeName   = content.TerritoryType.ValueNullable?.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Content_{content.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == content.RowId);

            ImGui.TableNextColumn();
            if (DService.Instance().Texture.TryGetFromGameIcon(new(content.ContentType.ValueNullable?.Icon ?? 0), out var icon))
                ImGui.Image(icon.GetWrapOrEmpty().Handle, ScaledVector2(20f));

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(content.ClassJobLevelRequired.ToString());

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{contentName}##Content_{content.RowId}",
                    false,
                    ImGuiSelectableFlags.SpanAllColumns
                ))
            {
                SelectedID  = content.RowId;
                selectState = true;
            }

            if (DService.Instance().Texture.TryGetFromGameIcon(new(content.Image), out var image) && ImGui.IsItemHovered())
            {
                using (ImRaii.Tooltip())
                    ImGui.Image(image.GetWrapOrEmpty().Handle, image.GetWrapOrEmpty().Size / 2);
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(placeName);
        }
    }

    public override bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItems.Count == 0
                          ? string.Empty
                          : $"[{SelectedItems.Count}] {SelectedItems.First().Name.ToString()} ({SelectedItems.First().RowId})...";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
        using var popup = ImRaii.Popup($"###Popup_{ID}");

        if (popup)
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("###Search", LuminaWrapper.GetAddonText(8128), ref SearchWord, 128))
                Searcher.Search(SearchWord);

            ImGui.Separator();

            var       tableSize = new Vector2(ImGui.GetContentRegionAvail().X, 0);
            using var table     = ImRaii.Table("###Table", 5, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("Checkbox",  ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Icon",      ImGuiTableColumnFlags.WidthFixed,   20f * GlobalFontScale);
                ImGui.TableSetupColumn("Level",     ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize(LuminaWrapper.GetAddonText(335)).X);
                ImGui.TableSetupColumn("DutyName",  ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("PlaceName", ImGuiTableColumnFlags.WidthStretch, 40);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(14098));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));

                foreach (var content in SelectedItems)
                    Render(content);

                foreach (var content in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(content.RowId)) continue;
                    Render(content);
                }
            }
        }

        return selectState;

        void Render(ContentFinderCondition content)
        {
            var contentName = content.Name.ToString();
            var placeName   = content.TerritoryType.ValueNullable?.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Content_{content.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedIDs.Contains(content.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(content.RowId))
                    SelectedIDs.Add(content.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            if (DService.Instance().Texture.TryGetFromGameIcon(new(content.ContentType.ValueNullable?.Icon ?? 0), out var icon))
                ImGui.Image(icon.GetWrapOrEmpty().Handle, ScaledVector2(20f));

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(content.ClassJobLevelRequired.ToString());

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{contentName}##Content_{content.RowId}",
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                ))
            {
                if (!SelectedIDs.Remove(content.RowId))
                    SelectedIDs.Add(content.RowId);
                selectState = true;
            }

            if (DService.Instance().Texture.TryGetFromGameIcon(new(content.Image), out var image) && ImGui.IsItemHovered())
            {
                using (ImRaii.Tooltip())
                    ImGui.Image(image.GetWrapOrEmpty().Handle, image.GetWrapOrEmpty().Size / 2);
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(placeName);
        }
    }
}
