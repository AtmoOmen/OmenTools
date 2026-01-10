using System.Numerics;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Widgets;

public class StatusSelectCombo
{
    public LuminaSearcher<Status> Searcher { get; init; }
    public string                 ID       { get; init; }

    public Status SelectedStatus =>
        LuminaGetter.GetRow<Status>(SelectedStatusID).GetValueOrDefault();

    public List<Status> SelectedStatuses =>
        SelectedStatusIDs.Select(x => LuminaGetter.GetRow<Status>(x).GetValueOrDefault())
                         .Where(x => x.RowId > 0)
                         .ToList();

    public uint          SelectedStatusID  { get; set; }
    public HashSet<uint> SelectedStatusIDs { get; set; } = [];

    public string SearchWord = string.Empty;
    
    public StatusSelectCombo(string id, IEnumerable<Status> statuses = null)
    {
        ID = id;
        
        var data = statuses ?? PresetSheet.Statuses.Values;
        Searcher = new LuminaSearcher<Status>(data,
                                              [
                                                  x => x.RowId.ToString(),
                                                  x => x.Name.ToString(),
                                                  x => x.Description.ToString() ?? string.Empty,
                                              ],
                                              resultLimit: 200);
    }
    
    public bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");
        
        var selectState = false;
        
        var preview = SelectedStatus.RowId == 0
                          ? string.Empty
                          : $"{SelectedStatus.Name.ToString()} ({SelectedStatus.RowId})";
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
                ImGui.TableSetupColumn("Job",         ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("Status",      ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                if (SelectedStatus is { RowId: > 0 })
                    Render(SelectedStatus);
                
                ImGui.TableNextRow();
                
                foreach (var status in Searcher.SearchResult)
                {
                    if (status.RowId == SelectedStatusID) continue;
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
            ImGui.RadioButton(string.Empty, SelectedStatus.RowId == status.RowId);

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{jobName}##Status_{status.RowId}_{statusName}", SelectedStatus.RowId == status.RowId,
                                 ImGuiSelectableFlags.SpanAllColumns))
            {
                SelectedStatusID = status.RowId;
                selectState      = true;
            }
            ImGuiOm.ImGuiOm.TooltipHover($"{status.Description.ToString()}");

            ImGui.TableNextColumn();
            ImGuiOm.ImGuiOm.TextImage(statusName, texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
        }
    }

    public bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");
        
        var selectState = false;
        
        var preview = SelectedStatuses.Count == 0
                          ? string.Empty
                          : $"[{SelectedStatuses.Count}] {SelectedStatuses.First().Name.ToString()} ({SelectedStatuses.First().RowId})...";
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
                ImGui.TableSetupColumn("Checkbox", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Job",      ImGuiTableColumnFlags.WidthStretch, 40);
                ImGui.TableSetupColumn("Status",   ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                foreach (var status in SelectedStatuses)
                    Render(status);
                
                foreach (var status in Searcher.SearchResult)
                {
                    if (SelectedStatusIDs.Contains(status.RowId)) continue;
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
            var isSelected = SelectedStatusIDs.Contains(status.RowId);
            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedStatusIDs.Remove(status.RowId))
                    SelectedStatusIDs.Add(status.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{jobName}##Status_{status.RowId}_{statusName}", false, 
                                 ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups))
            {
                if (!SelectedStatusIDs.Remove(status.RowId))
                    SelectedStatusIDs.Add(status.RowId);
                selectState    = true;
            }
            ImGuiOm.ImGuiOm.TooltipHover($"{status.Description.ToString()}");

            ImGui.TableNextColumn();
            ImGuiOm.ImGuiOm.TextImage(statusName, texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
        }
    }
} 
