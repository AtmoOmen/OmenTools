using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Interop.Game.AddonEvent.Abstractions;

namespace OmenTools.Interop.Game.AddonEvent;

public unsafe class AddonTalkEvent : AddonEventBase
{
    public static bool ClickNext()
    {
        if (Talk == null) return false;

        var evt = stackalloc AtkEvent[1]
        {
            new()
            {
                Listener = (AtkEventListener*)Talk,
                State    = new() { StateFlags = (AtkEventStateFlags)132 },
                Target   = &AtkStage.Instance()->AtkEventTarget
            }
        };

        var data = stackalloc AtkEventData[1];
        Talk->ReceiveEvent(AtkEventType.MouseClick, 0, evt, data);
        return true;
    }
}
