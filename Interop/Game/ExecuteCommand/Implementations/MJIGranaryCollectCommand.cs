using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIGranaryCollectCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIGranaryCollect;

    /// <summary>
    ///     收取无人岛屯货仓库探索结果
    /// </summary>
    public void Collect(uint granaryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, granaryIndex);
}