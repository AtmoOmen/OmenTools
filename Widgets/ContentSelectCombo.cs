using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class ContentSelectCombo
{
    public LuminaSearcher<ContentFinderCondition> Searcher { get; init; }
    public string                                 ID       { get; init; }

    public ContentFinderCondition SelectedContent =>
        LuminaGetter.GetRow<ContentFinderCondition>(SelectedContentID).GetValueOrDefault();

    public List<ContentFinderCondition> SelectedContents =>
        SelectedContentIDs.Select(x => LuminaGetter.GetRow<ContentFinderCondition>(x).GetValueOrDefault())
                          .Where(x => x.RowId > 0)
                          .ToList();

    public uint          SelectedContentID  { get; set; }
    public HashSet<uint> SelectedContentIDs { get; set; } = [];

    public string SearchWord = string.Empty;

    public ContentSelectCombo(string id, IEnumerable<ContentFinderCondition> contents = null)
    {
        ID = id;

        var data = contents ?? PresetSheet.Contents.Values;
        Searcher = new LuminaSearcher<ContentFinderCondition>(data,
                                              [
                                                  x => x.RowId.ToString(),
                                                  x => x.Name.ToString(),
                                                  x => x.TerritoryType.ValueNullable?.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty,
                                              ],
                                              resultLimit: 200);
    }

    public bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedContent.RowId == 0
                          ? string.Empty
                          : $"{SelectedContent.Name.ToString()} ({SelectedContent.RowId})";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup("###Popup");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
        using var popup = ImRaii.Popup("###Popup");
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

                if (SelectedContent is { RowId: > 0 })
                    Render(SelectedContent);

                foreach (var content in Searcher.SearchResult)
                {
                    if (content.RowId == SelectedContentID) continue;
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
            ImGui.RadioButton(string.Empty, SelectedContent.RowId == content.RowId);

            ImGui.TableNextColumn();
            if (DService.Instance().Texture.TryGetFromGameIcon(new(content.ContentType.ValueNullable?.Icon ?? 0), out var icon))
                ImGui.Image(icon.GetWrapOrEmpty().Handle, ScaledVector2(20f));

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(content.ClassJobLevelRequired.ToString());

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{contentName}##Content_{content.RowId}", false, 
                                 ImGuiSelectableFlags.SpanAllColumns))
            {
                SelectedContentID = content.RowId;
                selectState       = true;
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

    public bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedContents.Count == 0
                          ? string.Empty
                          : $"[{SelectedContents.Count}] {SelectedContents.First().Name.ToString()} ({SelectedContents.First().RowId})...";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup("###Popup");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
        using var popup = ImRaii.Popup("###Popup");
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

                foreach (var content in SelectedContents)
                    Render(content);

                foreach (var content in Searcher.SearchResult)
                {
                    if (SelectedContentIDs.Contains(content.RowId)) continue;
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
            var isSelected = SelectedContentIDs.Contains(content.RowId);
            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedContentIDs.Remove(content.RowId))
                    SelectedContentIDs.Add(content.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            if (DService.Instance().Texture.TryGetFromGameIcon(new(content.ContentType.ValueNullable?.Icon ?? 0), out var icon))
                ImGui.Image(icon.GetWrapOrEmpty().Handle, ScaledVector2(20f));

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(content.ClassJobLevelRequired.ToString());

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{contentName}##Content_{content.RowId}", isSelected, 
                                 ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups))
            {
                if (!SelectedContentIDs.Remove(content.RowId))
                    SelectedContentIDs.Add(content.RowId);
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
