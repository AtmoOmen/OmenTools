using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestDuelCommand : ExecuteCommandBase
{
    /// <summary>
    ///     确认决斗申请
    /// </summary>
    public static void Confirm() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDuel);

    /// <summary>
    ///     取消决斗申请
    /// </summary>
    public static void Cancel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDuel, 1);

    /// <summary>
    ///     强制取消决斗申请
    /// </summary>
    public static void ForceCancel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDuel, 4);
}
