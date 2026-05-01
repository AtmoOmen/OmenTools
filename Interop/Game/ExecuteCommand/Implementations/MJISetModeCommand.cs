using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJISetModeCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJISetMode;

    /// <summary>
    ///     切换无人岛模式
    /// </summary>
    public void Set(Mode mode) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)mode);

    public enum Mode : uint
    {
        Free = 0,
        Gather = 1,
        Sow = 2,
        Water = 3,
        Remove = 4,
        Feed = 6,
        Pet = 7,
        Beckon = 8,
        Capture = 9
    }
}