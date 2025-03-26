using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Infos;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static bool ClickTalk()
    {
        if (Talk == null) return false;
        
        var evt = stackalloc AtkEvent[1]
        {
            new()
            {
                Listener = (AtkEventListener*)Talk,
                State    = new() { StateFlags = (AtkEventStateFlags)132 },
                Target   = &AtkStage.Instance()->AtkEventTarget,
            },
        };
        
        var data = stackalloc AtkEventData[1];
        Talk->ReceiveEvent(AtkEventType.MouseClick, 0, evt, data);
        return true;
    }
    
    public static bool ClickSelectYesnoConfirm(string? textToContain = null)
    {
        if (!IsAddonAndNodesReady(SelectYesno)) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ExtractText();
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain, StringComparison.OrdinalIgnoreCase)) return false;
        }

        var addon = (AddonSelectYesno*)SelectYesno;
        addon->ConfirmCheckBox->ClickAddonCheckBox(SelectYesno, 3);
        return true;
    }
    
    public static bool ClickSelectYesnoNo(string? textToContain = null)
    {
        if (!IsAddonAndNodesReady(SelectYesno)) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ExtractText();
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain, StringComparison.OrdinalIgnoreCase)) return false;
        }

        var addon = (AddonSelectYesno*)SelectYesno;
        addon->NoButton->ClickAddonButton(SelectYesno);
        return true;
    }
    
    public static bool ClickSelectYesnoYes(string? textToContain = null)
    {
        if (!IsAddonAndNodesReady(SelectYesno)) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ExtractText();
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain, StringComparison.OrdinalIgnoreCase)) return false;
        }

        var addon = (AddonSelectYesno*)SelectYesno;
        addon->YesButton->ClickAddonButton(SelectYesno);
        return true;
    }
    
    public static bool ClickContextMenu(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(ContextMenu)) return false;
        if (!TryScanContextMenuText(ContextMenu, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(string text)
    {
        if (!IsAddonAndNodesReady(ContextMenu)) return false;
        if (!TryScanContextMenuText(ContextMenu, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(int index)
    {
        if (!IsAddonAndNodesReady(ContextMenu)) return false;

        Callback(ContextMenu, true, 0, index, 0U, 0, 0);
        return true;
    }

    public static bool ClickSelectString(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(SelectString)) return false;
        if (!TryScanSelectStringText(SelectString, text, out var index)) return false;

        return ClickSelectString(index);
    }

    public static bool ClickSelectString(string text)
    {
        if (!IsAddonAndNodesReady(SelectString)) return false;
        if (!TryScanSelectStringText(SelectString, text, out var index)) return false;

        return ClickSelectString(index);
    }

    public static bool ClickSelectString(int index)
    {
        if (!IsAddonAndNodesReady(SelectString)) return false;

        Callback(SelectString, true, index);
        return true;
    }

    public static bool ClickSelectIconString(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(SelectIconString)) return false;
        if (!TryScanSelectIconStringText(SelectIconString, text, out var index)) return false;

        Callback(SelectIconString, true, index);
        return true;
    }

    public static bool ClickSelectIconString(string text)
    {
        if (!IsAddonAndNodesReady(SelectIconString)) return false;

        if (!TryScanSelectIconStringText(SelectIconString, text, out var index)) return false;

        Callback(SelectIconString, true, index);
        return true;
    }

    public static bool ClickSelectIconString(int index)
    {
        if (!IsAddonAndNodesReady(SelectIconString)) return false;

        Callback(SelectIconString, true, index);
        return true;
    }

    public static void ClickAddonComponent(
        AtkUnitBase* addon, AtkComponentNode* target, uint which, EventType type, EventData? eventData = null, InputData? inputData = null)
    {
        eventData ??= EventData.ForNormalTarget(target, addon);
        inputData ??= InputData.Empty();

        InvokeReceiveEvent(&addon->AtkEventListener, type, which, eventData, inputData);
    }

    public static void ClickAddonComponent(
        AtkComponentBase* unitbase, AtkComponentNode* target, uint which, EventType type, EventData? eventData = null,
        InputData? inputData = null)
    {
        EventData? newEventData = null;
        InputData? newInputData = null;
        if (eventData == null)
            newEventData = EventData.ForNormalTarget(target, unitbase);

        if (inputData == null)
            newInputData = InputData.Empty();

        InvokeReceiveEvent(&unitbase->AtkEventListener, type, which, eventData ?? newEventData!,
            inputData ?? newInputData!);

        newEventData?.Dispose();
        newInputData?.Dispose();
    }
    
    public static void ClickAddonStage(AtkUnitBase* addon, uint which, EventType type = EventType.MOUSE_CLICK)
    {
        var target = AtkStage.Instance();

        var eventData = EventData.ForNormalTarget(target, addon);
        var inputData = InputData.Empty();

        InvokeReceiveEvent(&addon->AtkEventListener, type, which, eventData, inputData);
    }
    
    public static void ClickAddonButtonIndex(AtkUnitBase* addon, int nodeIndex)
    {
        var node = (AtkComponentButton*)addon->UldManager.NodeList[nodeIndex];
        node->ClickAddonButton(addon);
    }
}
