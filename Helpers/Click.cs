using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Infos;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    private delegate        byte                 FireCallbackDelegate(AtkUnitBase* addon, uint valueCount, AtkValue* values, byte updateState);
    private static readonly FireCallbackDelegate FireCallback = new CompSig("E8 ?? ?? ?? ?? 0F B6 F0 48 8D 5C 24").GetDelegate<FireCallbackDelegate>();

    public delegate nint ReceiveEventDelegate(AtkEventListener* eventListener, EventType eventType, uint which, void* eventData, void* inputData);
    
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
    
    public static bool ClickSelectYesnoConfirm(IReadOnlyList<string> text) => 
        text.Select(ClickSelectYesnoConfirm).Any(x => x);
    
    public static bool ClickSelectYesnoConfirm(string? textToContain = null)
    {
        if (!IsAddonAndNodesReady(SelectYesno)) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ExtractText().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain.Replace("\n", string.Empty), 
                                                                   StringComparison.OrdinalIgnoreCase))
                return false;
        }

        var addon = (AddonSelectYesno*)SelectYesno;
        addon->ConfirmCheckBox->ClickAddonCheckBox(SelectYesno, 3);
        return true;
    }
    
    public static bool ClickSelectYesnoNo(IReadOnlyList<string> text) => 
        text.Select(ClickSelectYesnoNo).Any(x => x);
    
    public static bool ClickSelectYesnoNo(string? textToContain = null)
    {
        if (!IsAddonAndNodesReady(SelectYesno)) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ExtractText().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain.Replace("\n", string.Empty), 
                                                                   StringComparison.OrdinalIgnoreCase)) 
                return false;
        }

        Callback(SelectYesno, true, 1);
        return true;
    }
    
    public static bool ClickSelectYesnoYes(IReadOnlyList<string> text) => 
        text.Select(ClickSelectYesnoYes).Any(x => x);

    public static bool ClickSelectYesnoYes(string? textToContain = null)
    {
        if (!IsAddonAndNodesReady(SelectYesno)) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ExtractText().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain.Replace("\n", string.Empty), 
                                                                   StringComparison.OrdinalIgnoreCase)) 
                return false;
        }

        Callback(SelectYesno, true, 0);
        return true;
    }
    
    public static bool ClickContextMenu(IReadOnlyList<string> text)
    {
        if (!IsAddonAndNodesReady(InfosOm.ContextMenu)) return false;
        if (!TryScanContextMenuText(InfosOm.ContextMenu, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(string text)
    {
        if (!IsAddonAndNodesReady(InfosOm.ContextMenu)) return false;
        if (!TryScanContextMenuText(InfosOm.ContextMenu, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(int index)
    {
        if (!IsAddonAndNodesReady(InfosOm.ContextMenu)) return false;

        Callback(InfosOm.ContextMenu, true, 0, index, 0U, 0, 0);
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

    public static void ClickAddonComponent(AtkUnitBase* addon, AtkComponentNode* target, uint which, 
                                           EventType type, EventData? eventData = null, InputData? inputData = null)
    {
        eventData ??= EventData.ForNormalTarget(target, addon);
        inputData ??= InputData.Empty();

        InvokeReceiveEvent(&addon->AtkEventListener, type, which, eventData, inputData);
    }

    public static void ClickAddonComponent(AtkComponentBase* component, AtkComponentNode* target, uint which, 
                                           EventType type, EventData? eventData = null, InputData? inputData = null)
    {
        eventData ??= EventData.ForNormalTarget(target, component);
        inputData ??= InputData.Empty();

        InvokeReceiveEvent(&component->AtkEventListener, type, which, eventData, inputData);
    }
    
    public static void ClickAddonStage(AtkUnitBase* addon, uint which, EventType type = EventType.MOUSE_CLICK)
    {
        var target = AtkStage.Instance();

        var eventData = EventData.ForNormalTarget(target, addon);
        var inputData = InputData.Empty();

        InvokeReceiveEvent(&addon->AtkEventListener, type, which, eventData, inputData);
    }
    
    public static void ClickAddonButtonIndex(AtkUnitBase* addon, int nodeIndex) => 
        ((AtkComponentButton*)addon->UldManager.NodeList[nodeIndex])->ClickAddonButton(addon);

    public static void ClickAddonButton(AtkUnitBase* addon, uint nodeID) =>
        ((AtkComponentButton*)addon->UldManager.SearchNodeById(nodeID))->ClickAddonButton(addon);

    public static ReceiveEventDelegate GetReceiveEvent(AtkEventListener* listener) => 
        Marshal.GetDelegateForFunctionPointer<ReceiveEventDelegate>(new nint(listener->VirtualTable->ReceiveEvent));

    public static void InvokeReceiveEvent(AtkEventListener* eventListener, EventType type, uint which, EventData eventData, InputData inputData) => 
        GetReceiveEvent(eventListener)(eventListener, type, which, eventData.Data, inputData.Data);
}
