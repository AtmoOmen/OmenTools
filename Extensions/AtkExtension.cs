using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

// 单独开一个文件方法太少了，就会扔到这里
public static unsafe class AtkExtension
{
    extension(scoped ref AtkImageNode node)
    {
        public uint GetIconID()
        {
            fixed (AtkImageNode* ptr = &node)
            {
                if (ptr == null) return 0;

                var partList = ptr->PartsList;
                if (partList == null) return 0;

                var parts = partList->Parts;
                if (parts == null) return 0;
                
                var asset = parts[ptr->PartId].UldAsset;
                if (asset == null) return 0;

                var resource = asset->AtkTexture.Resource;
                if (resource == null) return 0;
                
                return resource->IconId;
            }
        }
    }
    
    extension(scoped ref AtkTextNode node)
    {
        public Vector2 GetTextDrawSize(bool considerScale = false)
        {
            fixed (AtkTextNode* ptr = &node)
            {
                using var builder = new RentedSeStringBuilder();

                ushort sizeX = 0;
                ushort sizeY = 0;

                ptr->GetTextDrawSize(&sizeX, &sizeY, ptr->NodeText.StringPtr, considerScale: considerScale);
                return new Vector2(sizeX, sizeY);
            }
        }
    }
    
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

                btnRes.GetOwnerAddon()->ReceiveEvent(evt->State.EventType, (int)evt->Param, evt);
            }
        }
    }
}
