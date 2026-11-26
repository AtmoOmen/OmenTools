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
        public bool    IsScrollUp    => data.MouseData.WheelDirection >= 1;
        public bool    IsScrollDown  => data.MouseData.WheelDirection <= -1;
        public Vector2 MousePosition => new(data.MouseData.PosX, data.MouseData.PosY);

        /// <summary>
        ///     获取当前鼠标滚轮事件的有符号滚动量，正数表示向上，负数表示向下。
        /// </summary>
        public int ScrollValue => data.MouseData.WheelDirection;
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
        public bool    IsScrollUp    => eventData.GetAtkEventData()->MouseData.WheelDirection >= 1;
        public bool    IsScrollDown  => eventData.GetAtkEventData()->MouseData.WheelDirection <= -1;
        public Vector2 MousePosition => new(eventData.GetAtkEventData()->MouseData.PosX, eventData.GetAtkEventData()->MouseData.PosY);

        /// <summary>
        ///     获取当前鼠标滚轮事件的有符号滚动量，正数表示向上，负数表示向下。
        /// </summary>
        public int ScrollValue => eventData.GetAtkEventData()->MouseData.WheelDirection;
    }
}
