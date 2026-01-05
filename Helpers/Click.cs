using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

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
    
    public static bool ClickSelectYesnoConfirm(IReadOnlyList<string> text) => 
        text.Select(ClickSelectYesnoConfirm).Any(x => x);
    
    public static bool ClickSelectYesnoConfirm(string? textToContain = null)
    {
        if (!SelectYesno->IsAddonAndNodesReady()) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ToString().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain.Replace("\n", string.Empty), 
                                                                   StringComparison.OrdinalIgnoreCase))
                return false;
        }

        var addon = (AddonSelectYesno*)SelectYesno;
        addon->ConfirmCheckBox->Click(3);
        return true;
    }
    
    public static bool ClickSelectYesnoNo(IReadOnlyList<string> text) => 
        text.Select(ClickSelectYesnoNo).Any(x => x);
    
    public static bool ClickSelectYesnoNo(string? textToContain = null)
    {
        if (!SelectYesno->IsAddonAndNodesReady()) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ToString().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain.Replace("\n", string.Empty), 
                                                                   StringComparison.OrdinalIgnoreCase)) 
                return false;
        }

        SelectYesno->Callback(1);
        return true;
    }
    
    public static bool ClickSelectYesnoYes(IReadOnlyList<string> text) => 
        text.Select(ClickSelectYesnoYes).Any(x => x);

    public static bool ClickSelectYesnoYes(string? textToContain = null)
    {
        if (!SelectYesno->IsAddonAndNodesReady()) return false;
        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ToString().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains(textToContain.Replace("\n", string.Empty), 
                                                                   StringComparison.OrdinalIgnoreCase)) 
                return false;
        }

        SelectYesno->Callback(0);
        return true;
    }
    
    public static bool ClickContextMenu(IReadOnlyList<string> text)
    {
        if (!ContextMenuXIV->IsAddonAndNodesReady()) return false;
        if (!TryScanContextMenuText(ContextMenuXIV, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(string text)
    {
        if (!ContextMenuXIV->IsAddonAndNodesReady()) return false;
        if (!TryScanContextMenuText(ContextMenuXIV, text, out var index)) return false;

        return ClickContextMenu(index);
    }

    public static bool ClickContextMenu(int index)
    {
        if (!ContextMenuXIV->IsAddonAndNodesReady()) return false;

        ContextMenuXIV->Callback(0, index, 0U, 0, 0);
        return true;
    }

    public static bool ClickSelectString(IReadOnlyList<string> text)
    {
        if (!SelectString->IsAddonAndNodesReady()) return false;
        if (!TryScanSelectStringText(SelectString, text, out var index)) return false;

        return ClickSelectString(index);
    }

    public static bool ClickSelectString(string text)
    {
        if (!SelectString->IsAddonAndNodesReady()) return false;
        if (!TryScanSelectStringText(SelectString, text, out var index)) return false;

        return ClickSelectString(index);
    }

    public static bool ClickSelectString(int index)
    {
        if (!SelectString->IsAddonAndNodesReady()) return false;

        SelectString->Callback(index);
        return true;
    }

    public static bool ClickSelectIconString(IReadOnlyList<string> text)
    {
        if (!SelectIconString->IsAddonAndNodesReady()) return false;
        if (!TryScanSelectIconStringText(SelectIconString, text, out var index)) return false;

        SelectIconString->Callback(index);
        return true;
    }

    public static bool ClickSelectIconString(string text)
    {
        if (!SelectIconString->IsAddonAndNodesReady()) return false;

        if (!TryScanSelectIconStringText(SelectIconString, text, out var index)) return false;

        SelectIconString->Callback(index);
        return true;
    }

    public static bool ClickSelectIconString(int index)
    {
        if (!SelectIconString->IsAddonAndNodesReady()) return false;

        SelectIconString->Callback(index);
        return true;
    }
}
