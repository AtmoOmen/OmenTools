using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestDuelCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestDuel;

    /// <summary>
    ///     确认决斗申请
    /// </summary>
    public void Confirm() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag);

    /// <summary>
    ///     取消决斗申请
    /// </summary>
    public void Cancel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1);

    /// <summary>
    ///     强制取消决斗申请
    /// </summary>
    public void ForceCancel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 4);
}
