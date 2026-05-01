using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class HouseInteriorDesignChangeCommand : ExecuteCommandBase
{
    /// <summary>
    ///     更改房屋内部装修风格
    /// </summary>
    public static void Change(uint houseIndex, InteriorDesignStyle style) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.HouseInteriorDesignChange, houseIndex, (uint)style);

    public enum InteriorDesignStyle : uint
    {
        Mist         = 3,
        LavenderBeds = 6,
        Goblet       = 9,
        Shirogane    = 12,
        Empyreum     = 15,
        Simple       = 18
    }
}
