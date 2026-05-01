using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class LoadCraftLogCommand : ExecuteCommandBase
{
    /// <summary>
    ///     加载制作笔记数据
    /// </summary>
    public static void Load(uint jobIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.LoadCraftLog, jobIndex);
}
