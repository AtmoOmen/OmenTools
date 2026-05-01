using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SendDutySupportCommand : ExecuteCommandBase
{
    /// <summary>
    ///     发送剧情辅助器申请请求
    /// </summary>
    public static void Send(uint dawnStoryID, ReadOnlySpan<byte> memberUIParamIDs)
    {
        var param2 = Pack(memberUIParamIDs, 0, 4);
        var param3 = Pack(memberUIParamIDs, 4, 3);
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendDutySupport, dawnStoryID, param2, param3);
    }

    private static uint Pack(ReadOnlySpan<byte> values, int start, int count)
    {
        uint result = 0;
        var  end    = Math.Min(values.Length, start + count);

        for (var i = start; i < end; i++)
            result |= (uint)values[i] << (i - start) * 8;

        return result;
    }
}
