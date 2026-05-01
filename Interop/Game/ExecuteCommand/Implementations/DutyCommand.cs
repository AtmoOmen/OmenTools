using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class DutyCommand : ExecuteCommandBase
{
    /// <summary>
    ///     离开副本
    /// </summary>
    public static void Leave(LeaveDutyKind kind = LeaveDutyKind.Normal) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.LeaveDuty, (uint)kind);

    /// <summary>
    ///     发送剧情辅助器申请请求
    /// </summary>
    public static void DutySupport(uint dawnStoryID, ReadOnlySpan<byte> memberUIParamIDs)
    {
        var param2 = Pack(memberUIParamIDs, 0, 4);
        var param3 = Pack(memberUIParamIDs, 4, 3);
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendDutySupport, dawnStoryID, param2, param3);
    }

    /// <summary>
    ///     发送单人任务战斗请求
    /// </summary>
    public static void SoloQuestBattle(Difficulty difficulty = Difficulty.Normal) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StartSoloQuestBattle, (uint)difficulty);

    private static uint Pack(ReadOnlySpan<byte> values, int start, int count)
    {
        uint result = 0;
        var  end    = Math.Min(values.Length, start + count);

        for (var i = start; i < end; i++)
            result |= (uint)values[i] << (i - start) * 8;

        return result;
    }

    public enum Difficulty : uint
    {
        Normal   = 0,
        Easy     = 1,
        VeryEasy = 2
    }

    public enum LeaveDutyKind : uint
    {
        Normal   = 0,
        Inactive = 1
    }
}
