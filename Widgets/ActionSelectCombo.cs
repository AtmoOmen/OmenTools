using System.Numerics;
using Action = Lumina.Excel.Sheets.Action;

namespace OmenTools.Widgets;

public class ActionSelectCombo
{
    public LuminaSearcher<Action> Searcher { get; init; }
    public string                 ID       { get; init; }

    public Action SelectedAction =>
        LuminaGetter.GetRow<Action>(SelectedActionID).GetValueOrDefault();

    public List<Action> SelectedActions =>
        SelectedActionIDs.Select(x => LuminaGetter.GetRow<Action>(x).GetValueOrDefault())
                         .Where(x => x.RowId > 0)
                         .ToList();

    public uint          SelectedActionID  { get; set; }
    public HashSet<uint> SelectedActionIDs { get; set; } = [];

    public string SearchWord = string.Empty;
    
    public ActionSelectCombo(string id, IEnumerable<Action> actions = null)
    {
        ID = id;
        
        var data = actions ?? PresetSheet.PlayerActions.Values;
        Searcher = new LuminaSearcher<Action>(data,
                                              [
                                                  x => x.RowId.ToString(),
                                                  x => x.Name.ToString(),
                                                  x => x.ClassJob.ValueNullable?.Name.ToString() ?? string.Empty,
                                              ],
                                              resultLimit: 200);
    }
    
    public bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");
        
        var selectState = false;
        
        var preview = SelectedAction.RowId == 0
                          ? string.Empty
                          : $"[{SelectedAction.ClassJobCategory.ValueNullable?.Name.ToString()}] {SelectedAction.Name.ToString()} ({SelectedAction.RowId})";
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
            using var table     = ImRaii.Table("###Table", 4, ImGuiTableFlags.Borders, tableSize);
            if (table)
            {
                ImGui.TableSetupColumn("RadioButton", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Job",         ImGuiTableColumnFlags.WidthStretch, 20);
                ImGui.TableSetupColumn("Level",       ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize("1234").X);
                ImGui.TableSetupColumn("Action",      ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                if (SelectedAction is { RowId: > 0 })
                    Render(SelectedAction);
                
                foreach (var action in Searcher.SearchResult)
                {
                    if (action.RowId == SelectedActionID) continue;
                    Render(action);
                }
            }
        }

        return selectState;

        void Render(Action action)
        {
            if (!DService.Instance().Texture.TryGetFromGameIcon(new(action.Icon), out var texture)) return;
            
            var actionName = action.Name.ToString();
            var jobName    = action.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Action_{action.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedAction.RowId == action.RowId);

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{jobName}##Action_{action.RowId}_{action.Name.ToString()}", false, 
                                 ImGuiSelectableFlags.SpanAllColumns))
            {
                SelectedActionID = action.RowId;
                selectState      = true;
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(action.ClassJobLevel.ToString());

            ImGui.TableNextColumn();
            ImGuiOm.ImGuiOm.TextImage($"{actionName} ({action.RowId})", texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
        }
    }

    public bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");
        
        var selectState = false;
        
        var preview = SelectedActions.Count == 0
                          ? string.Empty
                          : $"[{SelectedActions.First().ClassJobCategory.ValueNullable?.Name.ToString()}] " +
                            $"{SelectedActions.First().Name.ToString()} "                           +
                            $"({SelectedActions.First().RowId})...";
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
            using var table     = ImRaii.Table("###Table", 4, ImGuiTableFlags.Borders, tableSize);
            if (table)
            {
                ImGui.TableSetupColumn("Checkbox", ImGuiTableColumnFlags.WidthFixed,   ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Job",      ImGuiTableColumnFlags.WidthStretch, 20);
                ImGui.TableSetupColumn("Level",    ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize("1234").X);
                ImGui.TableSetupColumn("Action",   ImGuiTableColumnFlags.WidthStretch, 50);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(294));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(1340));

                foreach (var action in SelectedActions)
                    Render(action);

                foreach (var action in Searcher.SearchResult)
                {
                    if (SelectedActionIDs.Contains(action.RowId)) continue;
                    Render(action);
                }
            }
        }

        return selectState;

        void Render(Action action)
        {
            if (!DService.Instance().Texture.TryGetFromGameIcon(new(action.Icon), out var texture)) return;
            
            var actionName = action.Name.ToString();
            var jobName    = action.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty;

            using var id = ImRaii.PushId($"Action_{action.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedActionIDs.Contains(action.RowId);
            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedActionIDs.Remove(action.RowId))
                    SelectedActionIDs.Add(action.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{jobName}##Action_{action.RowId}_{action.Name.ToString()}", isSelected, 
                                 ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups))
            {
                if (!SelectedActionIDs.Remove(action.RowId))
                    SelectedActionIDs.Add(action.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(action.ClassJobLevel.ToString());

            ImGui.TableNextColumn();
            ImGuiOm.ImGuiOm.TextImage(actionName, texture.GetWrapOrEmpty().Handle, new(ImGui.GetTextLineHeightWithSpacing()));
        }
    }
}
