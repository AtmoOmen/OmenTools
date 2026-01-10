using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class MountSelectCombo
{
    public LuminaSearcher<Mount> Searcher { get; init; }
    public string                ID       { get; init; }

    public Mount SelectedMount =>
        LuminaGetter.GetRow<Mount>(SelectedMountID).GetValueOrDefault();

    public List<Mount> SelectedMounts =>
        SelectedMountIDs.Select(x => LuminaGetter.GetRow<Mount>(x).GetValueOrDefault())
                        .Where(x => x.RowId > 0)
                        .ToList();

    public uint          SelectedMountID  { get; set; }
    public HashSet<uint> SelectedMountIDs { get; set; } = [];

    public string SearchWord = string.Empty;

    public MountSelectCombo(string id, IEnumerable<Mount> mounts = null)
    {
        ID = id;

        var data = mounts ?? LuminaGetter.Get<Mount>().Where(x => !string.IsNullOrEmpty(x.Singular.ToString()));
        Searcher = new LuminaSearcher<Mount>(data,
                                             [
                                                 x => x.RowId.ToString(),
                                                 x => x.Singular.ToString(),
                                                 x => (x.ExtraSeats + 1).ToString(),
                                             ],
                                             resultLimit: 200);
    }

    public bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedMount.RowId == 0
                          ? string.Empty
                          : $"{SelectedMount.Singular.ToString()} ({SelectedMount.RowId})";
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
            using var table     = ImRaii.Table("###Table", 2, ImGuiTableFlags.Borders, tableSize);
            if (table)
            {
                ImGui.TableSetupColumn("RadioButton", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Mount", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(6382));

                if (SelectedMount is { RowId: > 0 })
                    Render(SelectedMount);

                foreach (var mount in Searcher.SearchResult)
                {
                    if (mount.RowId == SelectedMountID) continue;
                    Render(mount);
                }
            }
        }

        return selectState;

        void Render(Mount mount)
        {
            using var id = ImRaii.PushId($"Mount_{mount.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedMount.RowId == mount.RowId);

            ImGui.TableNextColumn();
            if (DService.Instance().Texture.TryGetFromGameIcon(new(mount.Icon), out var texture))
            {
                if (ImGuiOm.ImGuiOm.SelectableImageWithText(texture.GetWrapOrEmpty().Handle, 
                                                            new(ImGui.GetTextLineHeight()),
                                                            mount.Singular.ToString(), 
                                                            false, 
                                                            ImGuiSelectableFlags.SpanAllColumns))
                {
                    SelectedMountID = mount.RowId;
                    selectState     = true;
                }
            }
            else
            {
                if (ImGui.Selectable($"{mount.Singular.ToString()}##Mount_{mount.RowId}", 
                                     false, 
                                     ImGuiSelectableFlags.SpanAllColumns))
                {
                    SelectedMountID = mount.RowId;
                    selectState     = true;
                }
            }
        }
    }

    public bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedMounts.Count == 0
                          ? string.Empty
                          : $"[{SelectedMounts.Count}] {SelectedMounts.First().Singular.ToString()} ({SelectedMounts.First().RowId})...";
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
            using var table     = ImRaii.Table("###Table", 2, ImGuiTableFlags.Borders, tableSize);
            if (table)
            {
                ImGui.TableSetupColumn("Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Mount",    ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(6382));

                foreach (var mount in SelectedMounts)
                    Render(mount);

                foreach (var mount in Searcher.SearchResult)
                {
                    if (SelectedMountIDs.Contains(mount.RowId)) continue;
                    Render(mount);
                }
            }
        }

        return selectState;

        void Render(Mount mount)
        {
            using var id = ImRaii.PushId($"Mount_{mount.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedMountIDs.Contains(mount.RowId);
            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedMountIDs.Remove(mount.RowId))
                    SelectedMountIDs.Add(mount.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            if (DService.Instance().Texture.TryGetFromGameIcon(new(mount.Icon), out var texture))
            {
                if (ImGuiOm.ImGuiOm.SelectableImageWithText(texture.GetWrapOrEmpty().Handle, 
                                                            new(ImGui.GetTextLineHeight()),
                                                            mount.Singular.ToString(), 
                                                            false,
                                                            ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups))
                {
                    if (!SelectedMountIDs.Remove(mount.RowId))
                        SelectedMountIDs.Add(mount.RowId);
                    selectState = true;
                }
            }
            else
            {
                if (ImGui.Selectable($"{mount.Singular.ToString()}##Mount_{mount.RowId}", 
                                     isSelected,
                                     ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups))
                {
                    if (!SelectedMountIDs.Remove(mount.RowId))
                        SelectedMountIDs.Add(mount.RowId);
                    selectState = true;
                }
            }
        }
    }

}

