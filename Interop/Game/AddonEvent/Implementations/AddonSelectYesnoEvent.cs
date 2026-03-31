using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools.Interop.Game.AddonEvent.Abstractions;

namespace OmenTools.Interop.Game.AddonEvent;

public unsafe class AddonSelectYesnoEvent : AddonEventBase
{
    public static bool CheckConfirm(IReadOnlyList<string> text) =>
        text.Select(CheckConfirm).Any(x => x);

    public static bool CheckConfirm(string? textToContain = null)
    {
        if (!SelectYesno->IsAddonAndNodesReady()) return false;

        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ToString().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) &&
                !text.Contains
                (
                    textToContain.Replace("\n", string.Empty),
                    StringComparison.OrdinalIgnoreCase
                ))
                return false;
        }

        var addon = (AddonSelectYesno*)SelectYesno;
        addon->ConfirmCheckBox->Click(3);
        return true;
    }

    public static bool ClickNo(IReadOnlyList<string> text) =>
        text.Select(ClickNo).Any(x => x);

    public static bool ClickNo(string? textToContain = null)
    {
        if (!SelectYesno->IsAddonAndNodesReady()) return false;

        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ToString().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) &&
                !text.Contains
                (
                    textToContain.Replace("\n", string.Empty),
                    StringComparison.OrdinalIgnoreCase
                ))
                return false;
        }

        SelectYesno->Callback(1);
        return true;
    }

    public static bool ClickYes(IReadOnlyList<string> text) =>
        text.Select(ClickYes).Any(x => x);

    public static bool ClickYes(string? textToContain = null)
    {
        if (!SelectYesno->IsAddonAndNodesReady()) return false;

        if (!string.IsNullOrWhiteSpace(textToContain))
        {
            var text = SelectYesno->GetTextNodeById(2)->NodeText.ToString().Replace("\n", string.Empty);
            if (!string.IsNullOrWhiteSpace(text) &&
                !text.Contains
                (
                    textToContain.Replace("\n", string.Empty),
                    StringComparison.OrdinalIgnoreCase
                ))
                return false;
        }

        SelectYesno->Callback(0);
        return true;
    }
}
