using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class FateSelectCombo : LuminaComboBase<Fate>
{
    public FateSelectCombo(string id, IEnumerable<Fate> fates = null) : base(id, null)
    {
        var data = fates ?? LuminaGetter.Get<Fate>().Where(x => !string.IsNullOrEmpty(x.Name.ToString())).OrderBy(x => x.ClassJobLevel);
        Searcher = new LuminaSearcher<Fate>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.Description.ToString(),
                x => x.Objective.ToString()
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
                ImGui.TableSetupColumn("Level",       ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize("1234").X);
                ImGui.TableSetupColumn("Fate",        ImGuiTableColumnFlags.WidthStretch, 40);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                foreach (var fate in Searcher.SearchResult)
                {
                    if (fate.RowId == SelectedID) continue;
                    Render(fate);
                }
            }
        }

        return selectState;

        void Render(Fate fate)
        {
            if (!DService.Instance().Texture.TryGetFromGameIcon(new(fate.Icon), out var texture)) return;

            var fateName = fate.Name.ToString();

            using var id = ImRaii.PushId($"Fate_{fate.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == fate.RowId);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(fate.ClassJobLevel.ToString());

            ImGui.TableNextColumn();

            if (ImGuiOm.ImGuiOm.SelectableImageWithText
                (
                    texture.GetWrapOrEmpty().Handle,
                    new(ImGui.GetTextLineHeight()),
                    $"{fateName}",
                    false,
                    ImGuiSelectableFlags.SpanAllColumns
                ))
            {
                SelectedID  = fate.RowId;
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover($"{fate.Description.ToString()}");
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
                ImGui.TableSetupColumn("Level",    ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize("1234").X);
                ImGui.TableSetupColumn("Fate",     ImGuiTableColumnFlags.WidthStretch, 40);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                foreach (var fate in SelectedItems)
                    Render(fate);

                foreach (var fate in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(fate.RowId)) continue;
                    Render(fate);
                }
            }
        }

        return selectState;

        void Render(Fate fate)
        {
            if (!DService.Instance().Texture.TryGetFromGameIcon(new(fate.Icon), out var texture)) return;

            var fateName = fate.Name.ToString();

            using var id = ImRaii.PushId($"Fate_{fate.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedIDs.Contains(fate.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(fate.RowId))
                    SelectedIDs.Add(fate.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(fate.ClassJobLevel.ToString());

            ImGui.TableNextColumn();

            if (ImGuiOm.ImGuiOm.SelectableImageWithText
                (
                    texture.GetWrapOrEmpty().Handle,
                    new(ImGui.GetTextLineHeight()),
                    $"{fateName}",
                    false,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                ))
            {
                if (!SelectedIDs.Remove(fate.RowId))
                    SelectedIDs.Add(fate.RowId);
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover($"{fate.Description.ToString()}");
        }
    }
}
