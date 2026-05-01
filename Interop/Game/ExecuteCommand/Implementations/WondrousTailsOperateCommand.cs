using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WondrousTailsOperateCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.WondrousTailsOperate;

    /// <summary>
    ///     天书奇谈再想想
    /// </summary>
    public void Rethink(uint index) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, index);
}