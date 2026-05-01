using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MountCommand : ExecuteCommandBase
{
    /// <summary>
    ///     下坐骑
    /// </summary>
    public static void Dismount(bool enqueue = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Dismount, enqueue ? 1U : 0U);

    /// <summary>
    ///     共同骑乘指定目标的位置
    /// </summary>
    public static void RidePillion(uint targetID, uint seatIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RidePillion, targetID, seatIndex);

    /// <summary>
    ///     赋予禁止骑乘坐骑状态
    /// </summary>
    public static void ApplyDisableMounting() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DisableMounting, 1);

    /// <summary>
    ///     移除禁止骑乘坐骑状态
    /// </summary>
    public static void RemoveDisableMounting() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DisableMounting);

    /// <summary>
    ///     赋予或取消禁止骑乘坐骑状态
    /// </summary>
    public static void ToggleDisableMounting(bool isDisabled) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DisableMounting, isDisabled ? 1U : 0U);
}
