using System.Numerics;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class MountSelectCombo : LuminaComboBase<Mount>
{
    public MountSelectCombo(string id, IEnumerable<Mount> mounts = null) : base(id, null)
    {
        var data = mounts ?? LuminaGetter.Get<Mount>().Where(x => !string.IsNullOrEmpty(x.Singular.ToString()));
        Searcher = new LuminaSearcher<Mount>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Singular.ToString(),
                x => (x.ExtraSeats + 1).ToString()
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
                          : $"{SelectedItem.Singular.ToString()} ({SelectedItem.RowId})";
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
            using var table     = ImRaii.Table("###Table", 2, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("RadioButton", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Mount",       ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(6382));

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                foreach (var mount in Searcher.SearchResult)
                {
                    if (mount.RowId == SelectedID) continue;
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
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == mount.RowId);

            ImGui.TableNextColumn();

            if (DService.Instance().Texture.TryGetFromGameIcon(new(mount.Icon), out var texture))
            {
                if (ImGuiOm.ImGuiOm.SelectableImageWithText
                    (
                        texture.GetWrapOrEmpty().Handle,
                        new(ImGui.GetTextLineHeight()),
                        mount.Singular.ToString(),
                        false,
                        ImGuiSelectableFlags.SpanAllColumns
                    ))
                {
                    SelectedID  = mount.RowId;
                    selectState = true;
                }
            }
            else
            {
                if (ImGui.Selectable
                    (
                        $"{mount.Singular.ToString()}##Mount_{mount.RowId}",
                        false,
                        ImGuiSelectableFlags.SpanAllColumns
                    ))
                {
                    SelectedID  = mount.RowId;
                    selectState = true;
                }
            }
        }
    }

    public override bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItems.Count == 0
                          ? string.Empty
                          : $"[{SelectedItems.Count}] {SelectedItems.First().Singular.ToString()} ({SelectedItems.First().RowId})...";
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
            using var table     = ImRaii.Table("###Table", 2, ImGuiTableFlags.Borders, tableSize);

            if (table)
            {
                ImGui.TableSetupColumn("Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
                ImGui.TableSetupColumn("Mount",    ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(6382));

                foreach (var mount in SelectedItems)
                    Render(mount);

                foreach (var mount in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(mount.RowId)) continue;
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
            var isSelected = SelectedIDs.Contains(mount.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(mount.RowId))
                    SelectedIDs.Add(mount.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();

            if (DService.Instance().Texture.TryGetFromGameIcon(new(mount.Icon), out var texture))
            {
                if (ImGuiOm.ImGuiOm.SelectableImageWithText
                    (
                        texture.GetWrapOrEmpty().Handle,
                        new(ImGui.GetTextLineHeight()),
                        mount.Singular.ToString(),
                        false,
                        ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                    ))
                {
                    if (!SelectedIDs.Remove(mount.RowId))
                        SelectedIDs.Add(mount.RowId);
                    selectState = true;
                }
            }
            else
            {
                if (ImGui.Selectable
                    (
                        $"{mount.Singular.ToString()}##Mount_{mount.RowId}",
                        isSelected,
                        ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                    ))
                {
                    if (!SelectedIDs.Remove(mount.RowId))
                        SelectedIDs.Add(mount.RowId);
                    selectState = true;
                }
            }
        }
    }
}
