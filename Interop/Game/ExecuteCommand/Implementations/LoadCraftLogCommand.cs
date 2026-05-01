using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class LoadCraftLogCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.LoadCraftLog;

    /// <summary>
    ///     加载制作笔记数据
    /// </summary>
    public void Load(uint jobIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, jobIndex);
}