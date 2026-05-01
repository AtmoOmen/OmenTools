using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WondrousTailsOperateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     天书奇谈再想想
    /// </summary>
    public static void Rethink(uint index) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WondrousTailsOperate, 0, index);
}
