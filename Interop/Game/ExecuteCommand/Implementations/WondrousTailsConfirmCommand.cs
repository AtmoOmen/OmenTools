using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WondrousTailsConfirmCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.WondrousTailsConfirm;

    /// <summary>
    ///     确认天书奇谈副本结果
    /// </summary>
    public void Confirm(uint index) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, index);
}