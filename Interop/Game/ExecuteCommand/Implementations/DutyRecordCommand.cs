using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class DutyRecordCommand : ExecuteCommandBase
{
    /// <summary>
    ///     开始任务回顾
    /// </summary>
    public static void Start() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StartDutyRecord);

    /// <summary>
    ///     结束任务回顾
    /// </summary>
    public static void End() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishDutyRecord);
}
