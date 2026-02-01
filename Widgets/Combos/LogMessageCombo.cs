using System.Numerics;
using Dalamud.Interface.Utility;
using Lumina.Excel.Sheets;

namespace OmenTools.Widgets;

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

    public override bool DrawRadio()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItem.RowId == 0
                          ? string.Empty
                          : $"{DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(SelectedItem.RowId)} " +
                            $"({SelectedItem.RowId})";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
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
                ImGui.TableSetupColumn("Text",        ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(2581));

                if (SelectedItem is { RowId: > 0 })
                    Render(SelectedItem);

                foreach (var logMessage in Searcher.SearchResult)
                {
                    if (logMessage.RowId == SelectedID) continue;
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
            ImGui.RadioButton(string.Empty, SelectedItem.RowId == logMessage.RowId);

            ImGui.TableNextColumn();

            var cursorPos = ImGui.GetCursorPos();

            if (ImGui.Selectable
                (
                    $"##LogMessage_{logMessage.RowId}",
                    SelectedItem.RowId == logMessage.RowId,
                    ImGuiSelectableFlags.SpanAllColumns
                ))
            {
                SelectedID  = logMessage.RowId;
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover
            (
                $"{LuminaWrapper.GetAddonText(4098)}:\n\t{logMessage.LogKind.Value.Format.ToMacroString().Trim()}\n\n{LuminaWrapper.GetAddonText(2581)}:\n\t{logMessage.Text.ToMacroString()}",
                40f * GlobalFontScale
            );

            ImGui.SameLine();
            ImGui.SetCursorPos(cursorPos);
            ImGuiHelpers.SeStringWrapped(DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(logMessage.RowId));
        }
    }

    public override bool DrawCheckbox()
    {
        using var drawID = ImRaii.PushId($"{ID}");

        var selectState = false;

        var preview = SelectedItems.Count == 0
                          ? string.Empty
                          : $"{DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(SelectedItems.First().RowId)} " +
                            $"({SelectedItems.First().RowId})...";
        if (ImGui.BeginCombo("###Combo", preview, ImGuiComboFlags.HeightLarge))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup($"###Popup_{ID}");

        ImGui.SetNextWindowSize(ScaledVector2(600f, 400f));
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
                ImGui.TableSetupColumn("Text",     ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LuminaWrapper.GetAddonText(2581));

                foreach (var logMessage in SelectedItems)
                    Render(logMessage);

                foreach (var logMessage in Searcher.SearchResult)
                {
                    if (SelectedIDs.Contains(logMessage.RowId)) continue;
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
            var isSelected = SelectedIDs.Contains(logMessage.RowId);

            if (ImGui.Checkbox(string.Empty, ref isSelected))
            {
                if (!SelectedIDs.Remove(logMessage.RowId))
                    SelectedIDs.Add(logMessage.RowId);
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
                if (!SelectedIDs.Remove(logMessage.RowId))
                    SelectedIDs.Add(logMessage.RowId);
                selectState = true;
            }

            ImGuiOm.ImGuiOm.TooltipHover
            (
                $"{LuminaWrapper.GetAddonText(4098)}:\n\t{logMessage.LogKind.Value.Format.ToMacroString().Trim()}\n\n{LuminaWrapper.GetAddonText(2581)}:\n\t{logMessage.Text.ToMacroString()}",
                40f * GlobalFontScale
            );

            ImGui.SameLine();
            ImGui.SetCursorPos(cursorPos);
            ImGuiHelpers.SeStringWrapped(DService.Instance().SeStringEvaluator.EvaluateFromLogMessage(logMessage.RowId));
        }
    }
}
