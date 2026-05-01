using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AroundRangeCommand : ExecuteCommandBase
{
    /// <summary>
    ///     设置角色显示范围
    /// </summary>
    public static void Set(Mode mode) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AroundRangeSetMode, (uint)mode);

    public enum Mode : uint
    {
        Standard = 0,
        Large    = 1,
        Maximum  = 2
    }
}
