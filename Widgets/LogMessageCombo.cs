using System.Numerics;
using Dalamud.Interface.Utility;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

public class LogMessageCombo
{
    public LuminaSearcher<LogMessage> Searcher { get; init; }
    public string                     ID       { get; init; }

    public LogMessage SelectedLogMessage =>
        LuminaGetter.GetRow<LogMessage>(SelectedLogMessageID).GetValueOrDefault();

    public List<LogMessage> SelectedLogMessages =>
        SelectedLogMessageIDs.Select(x => LuminaGetter.GetRow<LogMessage>(x).GetValueOrDefault())
                             .Where(x => x.RowId > 0)
                             .ToList();

    public uint          SelectedLogMessageID  { get; set; }
    public HashSet<uint> SelectedLogMessageIDs { get; set; } = [];

    public string SearchWord = string.Empty;

    public LogMessageCombo(string id, IEnumerable<LogMessage> logMessages = null)
    {
        ID = id;

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

    public bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedLogMessage.RowId == 0
                          ? string.Empty
                          : $"{DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(SelectedLogMessage.RowId)} " +
                            $"({SelectedLogMessage.RowId})";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup("###Popup");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
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
                ImGui.TableSetupColumn("Text",        ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(2581));

                if (SelectedLogMessage is { RowId: > 0 })
                    Render(SelectedLogMessage);

                foreach (var logMessage in Searcher.SearchResult)
                {
                    if (logMessage.RowId == SelectedLogMessageID) continue;
                    Render(logMessage);
                }
            }
        }

        return selectState;

        void Render(LogMessage logMessage)
        {
            using var id = ImRaii.PushId($"LogMessage_{logMessage.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.RadioButton(string.Empty, SelectedLogMessage.RowId == logMessage.RowId);

            ImGui.TableNextColumn();

            var cursorPos = ImGui.GetCursorPos();
            if (ImGui.Selectable
                (
                    $"##LogMessage_{logMessage.RowId}",
                    SelectedLogMessage.RowId == logMessage.RowId,
                    ImGuiSelectableFlags.SpanAllColumns
                ))
            {
                SelectedLogMessageID = logMessage.RowId;
                selectState          = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover($"{LuminaWrapper.GetAddonText(4098)}:\n\t{logMessage.LogKind.Value.Format.ToMacroString().Trim()}\n\n{LuminaWrapper.GetAddonText(2581)}:\n\t{logMessage.Text.ToMacroString()}", 40f * GlobalFontScale);

            ImGui.SameLine();
            ImGui.SetCursorPos(cursorPos);
            ImGuiHelpers.SeStringWrapped(DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(logMessage.RowId));
        }
    }

    public bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedLogMessages.Count == 0
                          ? string.Empty
                          : $"{DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(SelectedLogMessages.First().RowId)} " +
                            $"({SelectedLogMessages.First().RowId})...";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup("###Popup");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
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
                ImGui.TableSetupColumn("Text",     ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(2581));

                foreach (var logMessage in SelectedLogMessages)
                    Render(logMessage);

                foreach (var logMessage in Searcher.SearchResult)
                {
                    if (SelectedLogMessageIDs.Contains(logMessage.RowId)) continue;
                    Render(logMessage);
                }
            }
        }

        return selectState;

        void Render(LogMessage logMessage)
        {
            using var id = ImRaii.PushId($"LogMessage_{logMessage.RowId}");

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var isSelected = SelectedLogMessageIDs.Contains(logMessage.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedLogMessageIDs.Remove(logMessage.RowId))
                    SelectedLogMessageIDs.Add(logMessage.RowId);
                selectState = true;
            }

            ImGui.TableNextColumn();

            var cursorPos = ImGui.GetCursorPos();
            if (ImGui.Selectable
                (
                    $"##LogMessage_{logMessage.RowId}",
                    isSelected,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.DontClosePopups
                ))
            {
                if (!SelectedLogMessageIDs.Remove(logMessage.RowId))
                    SelectedLogMessageIDs.Add(logMessage.RowId);
                selectState = true;
            }
            
            ImGuiOm.ImGuiOm.TooltipHover($"{LuminaWrapper.GetAddonText(4098)}:\n\t{logMessage.LogKind.Value.Format.ToMacroString().Trim()}\n\n{LuminaWrapper.GetAddonText(2581)}:\n\t{logMessage.Text.ToMacroString()}", 40f * GlobalFontScale);

            ImGui.SameLine();
            ImGui.SetCursorPos(cursorPos);
            ImGuiHelpers.SeStringWrapped(DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(logMessage.RowId));
        }
    }
}
