using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

/// <summary>提供理符任务的名称、职业、等级和发行地点搜索及选择。</summary>
public class LeveSelectCombo : LuminaComboBase<Leve>
{
    /// <summary>创建理符任务选择控件。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="items">允许选择的理符任务；未指定时使用全部具名任务。</param>
    public LeveSelectCombo(string id, IEnumerable<Leve> items = null) : base(id, null) =>
        Searcher = new
        (
            items ?? LuminaGetter.Get<Leve>().Where(item => !item.Name.IsEmpty),
            [
                item => item.RowId.ToString(),
                item => item.Name.ToString(),
                item => item.ClassJobCategory.Value.Name.ToString(),
                item => item.ClassJobLevel.ToString(),
                item => item.PlaceNameIssued.Value.Name.ToString()
            ],
            resultLimit: int.MaxValue
        );

    /// <summary>获取或设置单选模式下选中的理符任务行标识。</summary>
    public override uint SelectedID { get; set; }

    /// <summary>获取或设置多选模式下选中的理符任务行标识集合。</summary>
    public override HashSet<uint> SelectedIDs { get; set; } = [];

    protected override string GetPreviewText(ComboSelectionMode mode)
    {
        var rowID = mode == ComboSelectionMode.Radio ?
                        SelectedID :
                        SelectedIDs.FirstOrDefault();
        if (rowID == 0 || !LuminaGetter.TryGetRow<Leve>(rowID, out var item))
            return string.Empty;

        return mode == ComboSelectionMode.Radio ?
                   $"[{item.ClassJobCategory.Value.Name}] \ue06a{item.ClassJobLevel.ToSESmallCount()} {item.Name}" :
                   $"[{SelectedIDs.Count}] {item.Name}";
    }

    protected override int GetTableColumnCount() => 5;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn("###Selected", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
        ImGui.TableSetupColumn("###Job",      ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight() * 4);
        ImGui.TableSetupColumn("###Level",    ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight() * 2);
        ImGui.TableSetupColumn("###Leve",     ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("###Place",    ImGuiTableColumnFlags.WidthStretch);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(334));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(335));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(574));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(416));
    }

    protected override bool DrawDataColumns(Leve item, ComboSelectionMode mode, bool isSelected)
    {
        ImGui.TableNextColumn();
        var iconID = 62100 +
                     (item.ClassJobCategory.RowId == 0 ?
                          44 :
                          item.ClassJobCategory.RowId - 1);

        if (ITextureProvider.Instance().TryGetFromGameIcon(new(iconID), out var icon))
        {
            ImGuiOm.TextImage(item.ClassJobCategory.Value.Name.ToString(), icon.GetWrapOrEmpty().Handle, new(ImGui.GetFrameHeight()));
        }
        else
            ImGui.TextUnformatted(item.ClassJobCategory.Value.Name.ToString());

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(item.ClassJobLevel.ToString());
        ImGui.TableNextColumn();
        var changed = ImGui.Selectable(item.Name.ToString(), isSelected, GetSelectableFlags(mode));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(item.PlaceNameIssued.Value.Name.ToString());
        return changed;
    }
}
