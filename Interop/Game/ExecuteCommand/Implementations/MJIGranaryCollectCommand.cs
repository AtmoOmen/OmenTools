using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIGranaryCollectCommand : ExecuteCommandBase
{
    /// <summary>
    ///     收取无人岛屯货仓库探索结果
    /// </summary>
    public static void Collect(uint granaryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIGranaryCollect, granaryIndex);
}
