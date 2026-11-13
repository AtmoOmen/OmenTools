using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

/// <summary>提供采集物数据行的搜索、等级展示及单选或多选交互。</summary>
public class GatheringItemSelectCombo : LuminaComboBase<GatheringItem>
{
    /// <summary>创建采集物下拉控件，可通过数据源限定可选范围。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="items">可选采集物；未指定时使用全部关联普通物品的采集物。</param>
    public GatheringItemSelectCombo(string id, IEnumerable<GatheringItem> items = null) : base(id, null)
        =>
            Searcher = new
            (
                items ?? LuminaGetter.Get<GatheringItem>().Where(item => item.Item.RowId is > 0 and < 1000000),
                [
                    item => item.RowId.ToString(),
                    item => item.Item.RowId.ToString(),
                    item => LuminaGetter.GetRow<Item>(item.Item.RowId).GetValueOrDefault().Name.ToString(),
                    item => item.GatheringItemLevel.Value.GatheringItemLevel.ToString()
                ],
                resultLimit: int.MaxValue
            );

    /// <summary>获取或设置单选模式下选中的 GatheringItem 行 ID。</summary>
    public override uint SelectedID { get; set; }

    /// <summary>获取或设置多选模式下选中的 GatheringItem 行 ID 集合。</summary>
    public override HashSet<uint> SelectedIDs { get; set; } = [];

    protected override string GetPreviewText(ComboSelectionMode mode)
    {
        var rowID = mode == ComboSelectionMode.Radio ?
                        SelectedID :
                        SelectedIDs.FirstOrDefault();
        if (rowID == 0)
            return string.Empty;

        var item = LuminaGetter.GetRow<GatheringItem>(rowID).GetValueOrDefault();
        var name = LuminaGetter.GetRow<Item>(item.Item.RowId).GetValueOrDefault().Name.ToString();
        return mode == ComboSelectionMode.Radio ?
                   name :
                   $"[{SelectedIDs.Count}] {name}";
    }

    protected override int GetTableColumnCount() => 3;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn("###Selected", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
        ImGui.TableSetupColumn("###Level",    ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight() * 2);
        ImGui.TableSetupColumn("###Item",     ImGuiTableColumnFlags.WidthStretch);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(520));
    }

    protected override bool DrawDataColumns(GatheringItem item, ComboSelectionMode mode, bool isSelected)
    {
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(item.GatheringItemLevel.Value.GatheringItemLevel.ToString());
        ImGui.TableNextColumn();

        var data = LuminaGetter.GetRow<Item>(item.Item.RowId).GetValueOrDefault();
        return ITextureProvider.Instance().TryGetFromGameIcon(new(data.Icon), out var texture) ?
                   ImGuiOm.SelectableImageWithText
                   (
                       texture.GetWrapOrEmpty().Handle,
                       new(ImGui.GetTextLineHeightWithSpacing()),
                       data.Name.ToString(),
                       isSelected,
                       GetSelectableFlags(mode)
                   ) :
                   ImGui.Selectable(data.Name.ToString(), isSelected, GetSelectableFlags(mode));
    }
}
