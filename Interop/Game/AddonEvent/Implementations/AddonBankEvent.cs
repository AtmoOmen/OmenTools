using OmenTools.Interop.Game.AddonEvent.Abstractions;

namespace OmenTools.Interop.Game.AddonEvent;

public unsafe class AddonBankEvent : AddonEventBase
{
    public static int RetainerGilAmount
    {
        get
        {
            if (!Bank->IsAddonAndNodesReady())
                return -1;

            return Bank->AtkValues[6].Int;
        }
    }

    public static void SwitchMode()
    {
        if (!Bank->IsAddonAndNodesReady()) return;
        Bank->Callback(2, 0);
    }

    public static void SetNumber(uint amount)
    {
        if (!Bank->IsAddonAndNodesReady()) return;
        Bank->Callback(3, amount);
    }

    public static void ClickConfirm()
    {
        if (!Bank->IsAddonAndNodesReady()) return;
        Bank->Callback(0, 0);
    }

    public static void ClickCancel()
    {
        if (!Bank->IsAddonAndNodesReady()) return;
        Bank->Callback(1, 0);
    }
}
