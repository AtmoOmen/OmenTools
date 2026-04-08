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

    protected override string GetPreviewText(ComboSelectionMode mode)
    {
        if (mode == ComboSelectionMode.Radio)
        {
            return SelectedItem.RowId == 0
                       ? string.Empty
                       : $"[{SelectedItem.LevelItem.RowId}] {SelectedItem.Name.ToString()} ({SelectedItem.RowId})";
        }

        return SelectedItems.Count == 0
                   ? string.Empty
                   : $"[{SelectedItems.First().LevelItem.RowId}] {SelectedItems.First().Name.ToString()} ({SelectedItems.First().RowId})...";
    }

    protected override Vector2 GetPopupSize() =>
        ScaledVector2(560f, 400f);

    protected override int GetTableColumnCount() =>
        3;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("ItemLevel", ImGuiTableColumnFlags.WidthFixed,   ImGui.CalcTextSize($"14{LuminaWrapper.GetAddonText(7873)}").X);
        ImGui.TableSetupColumn("Item",      ImGuiTableColumnFlags.WidthStretch, 80);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(7873));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(520));
    }

    protected override bool DrawDataColumns(Item item, ComboSelectionMode mode, bool isSelected)
    {
        var itemName     = item.Name.ToString();
        var description  = item.Description.ToString();
        var displayText  = $"{itemName} ({item.RowId})";
        var selectableID = $"##Item_{item.RowId}_{itemName}";

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(item.LevelItem.RowId.ToString());

        ImGui.TableNextColumn();

        var clicked = DService.Instance().Texture.TryGetFromGameIcon(new(item.Icon), out var texture)
                          ? ImGuiOm.SelectableImageWithText
                          (
                              texture.GetWrapOrEmpty().Handle,
                              new(ImGui.GetTextLineHeightWithSpacing()),
                              displayText,
                              isSelected,
                              GetSelectableFlags(mode)
                          )
                          : ImGui.Selectable
                          (
                              $"{displayText}{selectableID}",
                              isSelected,
                              GetSelectableFlags(mode)
                          );

        if (!string.IsNullOrWhiteSpace(description))
            ImGuiOm.TooltipHover(description);

        return clicked;
    }
}
