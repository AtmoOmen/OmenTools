using Dalamud.Interface.Utility;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.Combos;

public class LogMessageCombo : LuminaComboBase<LogMessage>
{
    public LogMessageCombo(string id, IEnumerable<LogMessage> logMessages = null) : base(id, null)
    {
        var data = logMessages ?? LuminaGetter.Get<LogMessage>();
        Searcher = new LuminaSearcher<LogMessage>
        (
            data,
            [
                x => x.RowId.ToString(),
                x => x.LogKind.Value.Format.ToMacroString(),
                x => x.Text.ToMacroString()
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
                       : $"{DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(SelectedItem.RowId)} ({SelectedItem.RowId})";
        }

        return SelectedItems.Count == 0
                   ? string.Empty
                   : $"{DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(SelectedItems.First().RowId)} ({SelectedItems.First().RowId})...";
    }

    protected override int GetTableColumnCount() =>
        2;

    protected override void SetupColumns(ComboSelectionMode mode)
    {
        ImGui.TableSetupColumn
            (mode == ComboSelectionMode.Radio ? "RadioButton" : "Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing());
        ImGui.TableSetupColumn("Text", ImGuiTableColumnFlags.WidthStretch);
    }

    protected override void DrawHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LuminaWrapper.GetAddonText(2581));
    }

    protected override bool DrawDataColumns(LogMessage logMessage, ComboSelectionMode mode, bool isSelected)
    {
        ImGui.TableNextColumn();

        var cursorPos = ImGui.GetCursorPos();
        var clicked = ImGui.Selectable
        (
            $"##LogMessage_{logMessage.RowId}",
            isSelected,
            GetSelectableFlags(mode)
        );

        ImGuiOm.TooltipHover
        (
            $"{LuminaWrapper.GetAddonText(4098)}:\n\t{logMessage.LogKind.Value.Format.ToMacroString().Trim()}\n\n{LuminaWrapper.GetAddonText(2581)}:\n\t{logMessage.Text.ToMacroString()}",
            40f * GlobalUIScale
        );

        ImGui.SameLine();
        ImGui.SetCursorPos(cursorPos);
        ImGuiHelpers.SeStringWrapped(DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(logMessage.RowId));
        return clicked;
    }
}
