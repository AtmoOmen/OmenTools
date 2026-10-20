using System.Numerics;
using Dalamud.Game.Addon.Events.EventDataTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class AtkEventDataExtension
{
    extension
    (
        scoped in AtkEventData data
    )
    {
        public bool    IsLeftClick   => data.MouseData.ButtonId is 0;
        public bool    IsRightClick  => data.MouseData.ButtonId is 1;
        public bool    IsNoModifier  => data.MouseData.Modifier is 0;
        public bool    IsAltHeld     => data.MouseData.Modifier.HasFlag(ModifierFlag.Alt);
        public bool    IsControlHeld => data.MouseData.Modifier.HasFlag(ModifierFlag.Ctrl);
        public bool    IsShiftHeld   => data.MouseData.Modifier.HasFlag(ModifierFlag.Shift);
        public bool    IsDragging    => data.MouseData.Modifier.HasFlag(ModifierFlag.Dragging);
        public bool    IsScrollUp    => data.MouseData.WheelDirection is 1;
        public bool    IsScrollDown  => data.MouseData.WheelDirection is -1;
        public Vector2 MousePosition => new(data.MouseData.PosX, data.MouseData.PosY);
    }

    extension
    (
        AddonEventData eventData
    )
    {
        public AtkEventData* GetAtkEventData() =>
            (AtkEventData*)eventData.AtkEventDataPointer;

        public bool    IsLeftClick   => eventData.GetAtkEventData()->MouseData.ButtonId is 0;
        public bool    IsRightClick  => eventData.GetAtkEventData()->MouseData.ButtonId is 1;
        public bool    IsNoModifier  => eventData.GetAtkEventData()->MouseData.Modifier is 0;
        public bool    IsAltHeld     => eventData.GetAtkEventData()->MouseData.Modifier.HasFlag(ModifierFlag.Alt);
        public bool    IsControlHeld => eventData.GetAtkEventData()->MouseData.Modifier.HasFlag(ModifierFlag.Ctrl);
        public bool    IsShiftHeld   => eventData.GetAtkEventData()->MouseData.Modifier.HasFlag(ModifierFlag.Shift);
        public bool    IsDragging    => eventData.GetAtkEventData()->MouseData.Modifier.HasFlag(ModifierFlag.Dragging);
        public bool    IsScrollUp    => eventData.GetAtkEventData()->MouseData.WheelDirection is 1;
        public bool    IsScrollDown  => eventData.GetAtkEventData()->MouseData.WheelDirection is -1;
        public Vector2 MousePosition => new(eventData.GetAtkEventData()->MouseData.PosX, eventData.GetAtkEventData()->MouseData.PosY);
    }
}
