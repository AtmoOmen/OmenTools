using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class AtkComponentExtensions
{
    extension(scoped ref AtkComponentNode node)
    {
        public void OutlineNode()
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return;
                ((AtkResNode*)nodePtr)->OutlineNode();
            }
        }
        
        public Vector2 GetPosition()
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return Vector2.Zero;
                return ((AtkResNode*)nodePtr)->GetPosition();
            }
        }

        public Vector2 GetScale()
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return Vector2.One;
                return ((AtkResNode*)nodePtr)->GetScale();
            }
        }

        public Vector2 GetSize()
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return Vector2.One;
                return ((AtkResNode*)nodePtr)->GetSize();
            }
        }

        public bool GetVisibility()
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;
                return ((AtkResNode*)nodePtr)->GetVisibility();
            }
        }

        public NodeState GetNodeState()
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return default;
                return ((AtkResNode*)nodePtr)->GetNodeState();
            }
        }
        
        public bool SetSize(scoped in Vector2 size)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;
                return ((AtkResNode*)nodePtr)->SetSize(size);
            }
        }
        
        public bool SetSize(scoped in float value)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;
                return ((AtkResNode*)nodePtr)->SetSize(value);
            }
        }
        
        public bool SetSize(scoped in float width, scoped in float height)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;
                return ((AtkResNode*)nodePtr)->SetSize(width, height);
            }
        }
        
        public bool SetWidth(scoped in float value)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;
                return ((AtkResNode*)nodePtr)->SetWidth(value);
            }
        }
        
        public bool SetHeight(scoped in float value)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;
                return ((AtkResNode*)nodePtr)->SetHeight(value);
            }
        }
        
        public void SetPosition(scoped in Vector2 position)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return;
                ((AtkResNode*)nodePtr)->SetPosition(position);
            }
        }
        
        public void SetPosition(scoped in float value)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return;
                ((AtkResNode*)nodePtr)->SetPosition(value);
            }
        }
        
        public void SetPosition(scoped in float x, scoped in float y)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return;
                ((AtkResNode*)nodePtr)->SetPosition(x, y);
            }
        }
        
        public void SetPositionX(scoped in float value)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return;
                ((AtkResNode*)nodePtr)->SetPositionX(value);
            }
        }

        public void SetPositionY(scoped in float value)
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return;
                ((AtkResNode*)nodePtr)->SetPositionY(value);
            }
        }
        
        public AtkUnitBase* GetOwnerAddon()
        {
            fixed (AtkComponentNode* nodePtr = &node)
            {
                if (nodePtr == null) return null;
                
                return ((AtkResNode*)nodePtr)->GetOwnerAddon();
            }
        }
    }
    
    extension(scoped ref AtkComponentTextInput textInput)
    {
        public static bool TryGetActive(out AtkComponentTextInput* component, out AtkUnitBase* addon)
        {
            component = null;
            addon     = null;

            var raptureAtkModule = RaptureAtkModule.Instance();
            if (raptureAtkModule == null)
                return false;

            var textInputEventInterface = raptureAtkModule->TextInput.TargetTextInputEventInterface;
            if (textInputEventInterface == null)
                return false;

            var ownerNode = textInputEventInterface->GetOwnerNode();
            if (ownerNode == null || ownerNode->GetNodeType() != NodeType.Component)
                return false;

            var componentNode = (AtkComponentNode*)ownerNode;
            var componentBase = componentNode->Component;
            if (componentBase == null || componentBase->GetComponentType() != ComponentType.TextInput)
                return false;

            component = (AtkComponentTextInput*)componentBase;

            addon = component->OwnerAddon;
            if (addon == null)
                addon = component->ContainingAddon2;

            if (addon == null)
                addon = RaptureAtkUnitManager.Instance()->GetAddonByNode((AtkResNode*)component->OwnerNode);

            return addon != null;
        }

    }
    
    extension(scoped ref AtkComponentRadioButton target)
    {
        public void Click(int which, AtkEventType type = AtkEventType.ButtonClick)
        {
            fixed (AtkComponentRadioButton* button = &target)
            {
                if (button == null) return;
                button->OwnerNode->GetOwnerAddon()->ClickComponent(button->OwnerNode, which, type);
            }
        }

        public void Click()
        {
            fixed (AtkComponentRadioButton* button = &target)
            {
                if (button == null) return;

                var addon = button->OwnerNode->GetOwnerAddon();
                var btnRes = button->OwnerNode->AtkResNode;
                var evt    = btnRes.AtkEventManager.Event;

                addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, evt);
            }
        }
    }

    extension(scoped ref AtkComponentCheckBox target)
    {
        public void Click(int which, AtkEventType type = AtkEventType.ButtonClick)
        {
            fixed (AtkComponentCheckBox* button = &target)
            {
                if (button == null) return;
                button->OwnerNode->GetOwnerAddon()->ClickComponent(button->OwnerNode, which, type);
            }
        }
    }

    extension(scoped ref AtkComponentDragDrop target)
    {
        public void Click(int which, AtkEventType type = AtkEventType.IconTextRollOut)
        {
            fixed (AtkComponentDragDrop* ptr = &target)
            {
                if (ptr == null) return;
                ptr->OwnerNode->GetOwnerAddon()->ClickComponent(ptr->OwnerNode, which, type);
            }
        }
    }

    extension(scoped ref AtkComponentButton target)
    {
        public void Click(AtkEvent* eventData)
        {
            fixed (AtkComponentButton* ptr = &target)
            {
                if (ptr == null) return;
                ptr->OwnerNode->GetOwnerAddon()->ReceiveEvent(eventData->State.EventType, (int)eventData->Param, eventData);
            }
        }

        public void Click()
        {
            fixed (AtkComponentButton* ptr = &target)
            {
                if (ptr == null) return;
                
                var ownerNode = ptr->OwnerNode;
                var evt       = ownerNode->AtkResNode.AtkEventManager.Event;

                ownerNode->GetOwnerAddon()->ReceiveEvent(evt->State.EventType, (int)evt->Param, evt);
            }
        }
    }

    extension(scoped ref AtkComponentBase component)
    {
        public void Click(
            AtkComponentNode*    target,
            int                  which,
            AtkEventType         type,
            ManagedAtkEvent?     eventData = null,
            ManagedAtkInputData? inputData = null)
        {
            fixed (AtkComponentBase* ptr = &component)
            {
                if (ptr == null) return;
                
                eventData ??= ManagedAtkEvent.ForNormalTarget(target, ptr);
                inputData ??= ManagedAtkInputData.Empty();

                ptr->AtkEventListener.ReceiveEvent(type, which, eventData.AtkEvent, inputData.AtkEventData);
            }
        }
    }
}
