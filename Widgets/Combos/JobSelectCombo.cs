using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class JobSelectCombo : LuminaComboBase<ClassJob>
{
    public JobSelectCombo(string id, IEnumerable<ClassJob> jobs = null) : base(id, null)
    {
        var data = jobs ??
                   LuminaGetter.Get<ClassJob>()
                               .Where(x => !string.IsNullOrEmpty(x.Name.ToString()));
        Searcher = new LuminaSearcher<ClassJob>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.Abbreviation.ToString(),
                x => x.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty
            ],
            x => x.OrderBy(d => d.Role)
                  .ThenBy(d => d.DohDolJobIndex)
                  .ThenBy(d => d.ClassJobCategory.RowId),
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
                          : $"[{SelectedItem.ClassJobCategory.ValueNullable?.Name.ToString()}] {SelectedItem.Name.ToString()} ({SelectedItem.RowId})";
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
            using var table     = ImRaii.Table("###Table", 4, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("RadioButton", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Icon",        ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Job",         ImGuiTableColumnFlags.WidthStretch, 70);
                ImGui.TableSetupColumn("Category",    ImGuiTableColumnFlags.WidthStretch, 30);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(7542));

                if (SelectedItem.RowId > 0)
                    Render(SelectedItem);

                foreach (var job in Searcher.SearchResult)
                {
                    if (job.RowId == SelectedID) continue;
                    Render(job);
                }
            }
        }

        return selectState;

        void Render(ClassJob job)
        {
            var iconID = 62100 + (job.RowId == 0 ? 44 : job.RowId);
            var icon   = ImageHelper.GetGameIcon(iconID);

            var jobName      = job.Name.ToString();
            var categoryName = job.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Job_{job.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == job.RowId);

            ImGui.TableNextColumn();
            ImGui.Image(icon.Handle, new(ImGui.GetTextLineHeightWithSpacing()));

            ImGui.TableNextColumn();

            if (ImGui.Selectable(jobName, false, ImGuiSelectableFlags.SpanAllColumns))
            {
                SelectedID  = job.RowId;
                selectState = true;
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(categoryName);
        }
    }

    public override bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItems.Count == 0
                          ? string.Empty
                          : $"[{SelectedItems.First().ClassJobCategory.ValueNullable?.Name.ToString()}] " +
                            $"{SelectedItems.First().Name.ToString()} "                                   +
                            $"({SelectedItems.First().RowId})...";
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
            using var table     = ImRaii.Table("###Table", 4, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("Checkbox", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Icon",     ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Job",      ImGuiTableColumnFlags.WidthStretch, 70);
                ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.WidthStretch, 30);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(7542));

                foreach (var job in SelectedItems)
                    Render(job);

                foreach (var job in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(job.RowId)) continue;
                    Render(job);
                }
            }
        }

        return selectState;

        void Render(ClassJob job)
        {
            var iconID = 62100 + (job.RowId == 0 ? 44 : job.RowId);
            var icon   = ImageHelper.GetGameIcon(iconID);

            var jobName      = job.Name.ToString();
            var categoryName = job.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Job_{job.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedIDs.Contains(job.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(job.RowId))
                    SelectedIDs.Add(job.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            ImGui.Image(icon.Handle, new(ImGui.GetTextLineHeightWithSpacing()));

            ImGui.TableNextColumn();

            if (ImGui.Selectable(jobName, isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups))
            {
                if (!SelectedIDs.Remove(job.RowId))
                    SelectedIDs.Add(job.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(categoryName);
        }
    }
}
