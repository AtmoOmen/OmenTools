using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AroundRangeSetModeCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.AroundRangeSetMode;

    /// <summary>
    ///     设置角色显示范围
    /// </summary>
    public void Set(Mode mode) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)mode);

    public enum Mode : uint
    {
        Standard = 0,
        Large = 1,
        Maximum = 2
    }
}