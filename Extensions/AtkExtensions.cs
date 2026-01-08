using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

// 单独开一个文件方法太少了，就会扔到这里
public static unsafe class AtkExtensions
{
    extension(scoped ref AtkCollisionNode target)
    {
        public void Click(AtkEvent* eventData)
        {
            fixed (AtkCollisionNode* ptr = &target)
            {
                if (ptr == null) return;
                ptr->AtkResNode.GetOwnerAddon()->ReceiveEvent(eventData->State.EventType, (int)eventData->Param, eventData);
            }
        }

        public void Click()
        {
            fixed (AtkCollisionNode* ptr = &target)
            {
                if (ptr == null) return;
                
                var btnRes = ptr->AtkResNode;
                var evt    = btnRes.AtkEventManager.Event;

                while (evt->State.EventType != AtkEventType.MouseClick)
                    evt = evt->NextEvent;

                btnRes.GetOwnerAddon()->ReceiveEvent(evt->State.EventType, (int)evt->Param,evt);
            }
        }
    }
}
