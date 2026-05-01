using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WondrousTailsConfirmCommand : ExecuteCommandBase
{
    /// <summary>
    ///     确认天书奇谈副本结果
    /// </summary>
    public static void Confirm(uint index) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WondrousTailsConfirm, index);
}
