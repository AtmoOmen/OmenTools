using System.Numerics;
using Dalamud.Game.Addon.Events.EventDataTypes;
using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Helpers;

public static unsafe class AtkExtension
{
    private delegate nint InvokeListenerDelegate(AtkUnitBase* addon, AtkEventType eventType, uint param, AtkEvent* atkEvent);
    private static readonly InvokeListenerDelegate InvokeListener = 
        new CompSig("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 0F B7 FA").GetDelegate<InvokeListenerDelegate>();

    extension(AtkUldManager manager)
    {
        public List<nint> SearchSimpleNodesByType(NodeType type)
        {
            var result = new List<nint>();
            for (var i = 0; i < manager.NodeListCount; i++)
            {
                var node = manager.NodeList[i];
                // 非 SimpleNode
                if (node == null || (int)node->Type > 1000) continue;
                if (node->Type != type) continue;

                result.Add((nint)node);
            }

            return result;
        }

        public T* SearchSimpleNodeByType<T>(NodeType type) where T : unmanaged
        {
            for (var i = 0; i < manager.NodeListCount; i++)
            {
                var node = manager.NodeList[i];
                // 非 SimpleNode
                if (node == null || (int)node->Type > 1000) continue;
                if (node->Type != type) continue;

                return (T*)node;
            }

            return null;
        }

        public nint SearchSimpleNodeByType(NodeType type)
        {
            for (var i = 0; i < manager.NodeListCount; i++)
            {
                var node = manager.NodeList[i];
                // 非 SimpleNode
                if (node == null || (int)node->Type > 1000) continue;
                if (node->Type != type) continue;

                return (nint)node;
            }

            return nint.Zero;
        }

        public List<nint> SearchComponentNodesByType(ComponentType type)
        {
            var result = new List<nint>();
            for (var i = 0; i < manager.NodeListCount; i++)
            {
                var node = manager.NodeList[i];
                // 非 ComponentNode
                if (node == null || (int)node->Type < 1000) continue;

                var componentNode = (AtkComponentNode*)node;
                var componentInfo = componentNode->Component->UldManager;
                var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                if (objectInfo == null || objectInfo->ComponentType != type) continue;

                result.Add((nint)componentNode->Component);
            }

            return result;
        }

        public T* SearchComponentNodeByType<T>(ComponentType type) where T : unmanaged
        {
            for (var i = 0; i < manager.NodeListCount; i++)
            {
                var node = manager.NodeList[i];
                // 非 ComponentNode
                if (node == null || (int)node->Type < 1000) continue;

                var componentNode = (AtkComponentNode*)node;
                var componentInfo = componentNode->Component->UldManager;
                var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                if (objectInfo == null || objectInfo->ComponentType != type) continue;

                return (T*)componentNode->Component;
            }

            return null;
        }

        public nint SearchComponentNodeByType(ComponentType type)
        {
            for (var i = 0; i < manager.NodeListCount; i++)
            {
                var node = manager.NodeList[i];
                // 非 ComponentNode
                if (node == null || (int)node->Type < 1000) continue;

                var componentNode = (AtkComponentNode*)node;
                var componentInfo = componentNode->Component->UldManager;
                var objectInfo    = (AtkUldComponentInfo*)componentInfo.Objects;
                if (objectInfo == null || objectInfo->ComponentType != type) continue;

                return (nint)componentNode->Component;
            }

            return nint.Zero;
        }
    }

    public static AtkUnitBase* ToAtkUnitBase(this nint ptr) =>
        (AtkUnitBase*)ptr;

    public static AtkUnitBase* ToAtkUnitBase(this AtkUnitBasePtr wrapper) =>
        (AtkUnitBase*)wrapper.Address;

    public static void ClickAddonRadioButton(this AtkComponentRadioButton target, AtkUnitBase* addon, uint which, EventType type = EventType.CHANGE)
        => ClickAddonComponent(addon, (&target)->OwnerNode, which, type);

    public static void ClickAddonCheckBox(this AtkComponentCheckBox target, AtkUnitBase* addon, uint which, EventType type = EventType.CHANGE)
        => ClickAddonComponent(addon, (&target)->AtkComponentButton.AtkComponentBase.OwnerNode, which, type);

    public static void ClickAddonDragDrop(this AtkComponentDragDrop target, AtkUnitBase* addon, uint which, EventType type = EventType.ICON_TEXT_ROLL_OUT)
        => ClickAddonComponent(addon, (&target)->AtkComponentBase.OwnerNode, which, type);

    public static void ClickAddonButton(this AtkComponentButton target, AtkUnitBase* addon, AtkEvent* eventData)
        => InvokeListener(addon, eventData->State.EventType, eventData->Param, eventData);

    public static void ClickAddonButton(this AtkCollisionNode target, AtkUnitBase* addon, AtkEvent* eventData)
        => InvokeListener(addon, eventData->State.EventType, eventData->Param, eventData);

    public static void ClickAddonButton(this AtkComponentButton target, AtkUnitBase* addon)
    {
        var btnRes = target.AtkComponentBase.OwnerNode->AtkResNode;
        var evt    = btnRes.AtkEventManager.Event;

        addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, btnRes.AtkEventManager.Event);
    }

    public static void ClickAddonButton(this AtkCollisionNode target, AtkUnitBase* addon)
    {
        var btnRes = target.AtkResNode;
        var evt    = btnRes.AtkEventManager.Event;

        while (evt->State.EventType != AtkEventType.MouseClick)
            evt = evt->NextEvent;

        addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, btnRes.AtkEventManager.Event);
    }

    public static void ClickRadioButton(this AtkComponentRadioButton target, AtkUnitBase* addon)
    {
        var btnRes = target.OwnerNode->AtkResNode;
        var evt    = btnRes.AtkEventManager.Event;

        addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, btnRes.AtkEventManager.Event);
    }

    extension(AtkEventData data)
    {
        public bool    IsLeftClick   => data.MouseData.ButtonId is 0;
        public bool    IsRightClick  => data.MouseData.ButtonId is 1;
        public bool    IsNoModifier  => data.MouseData.Modifier is 0;
        public bool    IsAltHeld     => data.MouseData.Modifier.HasFlag(AtkEventData.AtkMouseData.ModifierFlag.Alt);
        public bool    IsControlHeld => data.MouseData.Modifier.HasFlag(AtkEventData.AtkMouseData.ModifierFlag.Ctrl);
        public bool    IsShiftHeld   => data.MouseData.Modifier.HasFlag(AtkEventData.AtkMouseData.ModifierFlag.Shift);
        public bool    IsDragging    => data.MouseData.Modifier.HasFlag(AtkEventData.AtkMouseData.ModifierFlag.Dragging);
        public bool    IsScrollUp    => data.MouseData.WheelDirection is 1;
        public bool    IsScrollDown  => data.MouseData.WheelDirection is -1;
        public Vector2 MousePosition => new(data.MouseData.PosX, data.MouseData.PosY);
    }
}
