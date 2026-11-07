using Lumina.Excel.Sheets;
using OmenTools.Info.Lumina;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

/// <summary>
///     以太之光选择组件，支持单选、多选以及按行编号、以太之光名称和所在区域名称搜索。
/// </summary>
public class AetheryteCombo : LuminaComboBase<Aetheryte>
{
    /// <summary>
    ///     创建以太之光选择组件，并建立可搜索的数据集合。
    /// </summary>
    /// <param name="id">组件在当前界面中的唯一标识。</param>
    /// <param name="aetherytes">可供选择的以太之光；未指定时使用游戏内全部具名以太之光。</param>
    public AetheryteCombo(string id, IEnumerable<Aetheryte>? aetherytes = null) : base(id, null) =>
        Searcher = new
        (
            aetherytes ?? Sheets.Aetherytes.Values.Where(aetheryte => aetheryte.IsAetheryte),
            [
                aetheryte => aetheryte.RowId.ToString(),
                aetheryte => aetheryte.PlaceName.Value.Name.ToString(),
                aetheryte => aetheryte.Territory.Value.ExtractPlaceName()
            ],
            resultLimit: int.MaxValue
        );

    /// <summary>
    ///     获取或设置单选模式下选中的以太之光行编号；零表示未选择。
    /// </summary>
    public override uint SelectedID { get; set; }

    /// <summary>
    ///     获取或设置多选模式下选中的以太之光行编号集合。
    /// </summary>
    public override HashSet<uint> SelectedIDs { get; set; } = [];

    protected override string GetPreviewText(ComboSelectionMode mode)
    {
        if (mode == ComboSelectionMode.Radio)
        {
            return SelectedID == 0 ?
                       string.Empty :
                       SelectedItem.PlaceName.Value.Name.ToString();
        }

        var items = SelectedItems;
        return items.Count == 0 ?
                   string.Empty :
                   $"[{items.Count}] {items[0].PlaceName.Value.Name.ToString()}";
    }

    protected override int GetTableColumnCount() => 3;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn("###Selected", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
        ImGui.TableSetupColumn("###Name",     ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("###Zone",     ImGuiTableColumnFlags.WidthStretch);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(6382));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(870));
    }

    protected override bool DrawDataColumns(Aetheryte item, ComboSelectionMode mode, bool isSelected)
    {
        ImGui.TableNextColumn();
        var clicked = ImGui.Selectable
        (
            $"{item.PlaceName.Value.Name.ToString()}",
            isSelected,
            GetSelectableFlags(mode)
        );

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(item.Territory.Value.ExtractPlaceName());
        return clicked;
    }
}
