using System.Numerics;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class ItemSelectCombo : LuminaComboBase<Item>
{
    public ItemSelectCombo(string id, IEnumerable<Item> items = null) : base(id, null)
    {
        var data = items ?? LuminaGetter.Get<Item>().Where(x => !string.IsNullOrEmpty(x.Name.ToString()));
        Searcher = new LuminaSearcher<Item>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.Name.ToString(),
                x => x.Description.ToString(),
                x => x.LevelEquip.ToString(),
                x => x.LevelItem.RowId.ToString(),
                x => x.ItemUICategory.ValueNullable?.Name.ToString() ?? string.Empty
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
                          : $"[{SelectedItem.LevelItem.RowId}] {SelectedItem.Name.ToString()} ({SelectedItem.RowId})";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(560f, 400f));
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
                ImGui.TableSetupColumn("ItemLevel",   ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize("1234").X);
                ImGui.TableSetupColumn("Item",        ImGuiTableColumnFlags.WidthStretch, 80);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted("品级");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted("物品");

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                foreach (var item in Searcher.SearchResult)
                {
                    if (item.RowId == SelectedID) continue;
                    Render(item);
                }
            }
        }

        return selectState;

        void Render(Item item)
        {
            var itemName     = item.Name.ToString();
            var description  = item.Description.ToString();
            var itemLevel    = item.LevelItem.RowId;
            var isSelected   = SelectedItem.RowId == item.RowId;
            var displayText  = $"{itemName} ({item.RowId})";
            var selectableID = $"##Item_{item.RowId}_{itemName}";

            using var id = ImRaii.PushId($"Item_{item.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, isSelected);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(itemLevel.ToString());

            ImGui.TableNextColumn();

            var clicked = false;
            if (DService.Instance().Texture.TryGetFromGameIcon(new(item.Icon), out var texture))
            {
                clicked = ImGuiOm.SelectableImageWithText
                (
                    texture.GetWrapOrEmpty().Handle,
                    new(ImGui.GetTextLineHeightWithSpacing()),
                    displayText,
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns
                );
            }
            else
            {
                clicked = ImGui.Selectable
                (
                    $"{displayText}{selectableID}",
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns
                );
            }

            if (!string.IsNullOrWhiteSpace(description))
                ImGuiOm.TooltipHover(description);

            if (!clicked) return;

            SelectedID  = item.RowId;
            selectState = true;
        }
    }

    public override bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItems.Count == 0
                          ? string.Empty
                          : $"[{SelectedItems.First().LevelItem.RowId}] {SelectedItems.First().Name.ToString()} ({SelectedItems.First().RowId})...";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(560f, 400f));
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
                ImGui.TableSetupColumn("ItemLevel", ImGuiTableColumnFlags.WidthFixed,  ImGui.CalcTextSize("1234").X);
                ImGui.TableSetupColumn("Item",      ImGuiTableColumnFlags.WidthStretch, 80);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted("品级");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted("物品");

                foreach (var item in SelectedItems)
                    Render(item);

                foreach (var item in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(item.RowId)) continue;
                    Render(item);
                }
            }
        }

        return selectState;

        void Render(Item item)
        {
            var itemName     = item.Name.ToString();
            var description  = item.Description.ToString();
            var itemLevel    = item.LevelItem.RowId;
            var isSelected   = SelectedIDs.Contains(item.RowId);
            var displayText  = $"{itemName} ({item.RowId})";
            var selectableID = $"##Item_{item.RowId}_{itemName}";

            using var id = ImRaii.PushId($"Item_{item.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(item.RowId))
                    SelectedIDs.Add(item.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(itemLevel.ToString());

            ImGui.TableNextColumn();

            var clicked = false;
            if (DService.Instance().Texture.TryGetFromGameIcon(new(item.Icon), out var texture))
            {
                clicked = ImGuiOm.SelectableImageWithText
                (
                    texture.GetWrapOrEmpty().Handle,
                    new(ImGui.GetTextLineHeightWithSpacing()),
                    displayText,
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                );
            }
            else
            {
                clicked = ImGui.Selectable
                (
                    $"{displayText}{selectableID}",
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                );
            }

            if (!string.IsNullOrWhiteSpace(description))
                ImGuiOm.TooltipHover(description);

            if (!clicked) return;

            if (!SelectedIDs.Remove(item.RowId))
                SelectedIDs.Add(item.RowId);
            selectState = true;
        }
    }
}
