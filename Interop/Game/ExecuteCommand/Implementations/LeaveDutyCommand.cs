using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class LeaveDutyCommand : ExecuteCommandBase
{
    /// <summary>
    ///     离开副本
    /// </summary>
    public static void Leave(LeaveDutyKind kind = LeaveDutyKind.Normal) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.LeaveDuty, (uint)kind);

    public enum LeaveDutyKind : uint
    {
        Normal   = 0,
        Inactive = 1
    }
}
