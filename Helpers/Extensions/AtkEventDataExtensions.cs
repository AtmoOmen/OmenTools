using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Helpers;

public static class AtkEventDataExtensions
{
    extension(scoped in AtkEventData data)
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
