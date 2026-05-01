using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WondrousTailsCommand : ExecuteCommandBase
{
    /// <summary>
    ///     确认天书奇谈副本结果
    /// </summary>
    public static void Confirm(uint index) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WondrousTailsConfirm, index);

    /// <summary>
    ///     天书奇谈再想想
    /// </summary>
    public static void Rethink(uint index) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WondrousTailsOperate, 0, index);
}
