using System.Numerics;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Widgets;

public class StatusSelectCombo : LuminaComboBase<Status>
{
    public StatusSelectCombo(string id, IEnumerable<Status> statuses = null) : base(id, null)
    {
        var data = statuses ?? PresetSheet.Statuses.Values;
        Searcher = new LuminaSearcher<Status>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.Description.ToString() ?? string.Empty
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
                ImGui.TableSetupColumn("Job",         ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("Status",      ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                ImGui.TableNextRow();

                foreach (var status in Searcher.SearchResult)
                {
                    if (status.RowId == SelectedID) continue;
                    Render(status);
                }
            }
        }

        return selectState;

        void Render(Status status)
        {
            if (!DService.Instance().Texture.TryGetFromGameIcon(new(status.Icon), out var texture)) return;

            var statusName = status.Name.ToString();
            var jobName    = status.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Status_{status.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == status.RowId);

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{jobName}##Status_{status.RowId}_{statusName}",
                    SelectedItem.RowId == status.RowId,
                    ImGuiSelectableFlags.SpanAllColumns
                ))
            {
                SelectedID  = status.RowId;
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover($"{status.Description.ToString()}");

            ImGui.TableNextColumn();
            ImGuiOm.ImGuiOm.TextImage(statusName, texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
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
                ImGui.TableSetupColumn("Job",      ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("Status",   ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                foreach (var status in SelectedItems)
                    Render(status);

                foreach (var status in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(status.RowId)) continue;
                    Render(status);
                }
            }
        }

        return selectState;

        void Render(Status status)
        {
            if (!DService.Instance().Texture.TryGetFromGameIcon(new(status.Icon), out var texture)) return;

            var statusName = status.Name.ToString();
            var jobName    = status.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Status_{status.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedIDs.Contains(status.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(status.RowId))
                    SelectedIDs.Add(status.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();

            if (ImGui.Selectable
                (
                    $"{jobName}##Status_{status.RowId}_{statusName}",
                    false,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                ))
            {
                if (!SelectedIDs.Remove(status.RowId))
                    SelectedIDs.Add(status.RowId);
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover($"{status.Description.ToString()}");

            ImGui.TableNextColumn();
            ImGuiOm.ImGuiOm.TextImage(statusName, texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
        }
    }
}
