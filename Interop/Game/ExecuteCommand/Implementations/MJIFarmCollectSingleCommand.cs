using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIFarmCollectSingleCommand : ExecuteCommandBase
{
    /// <summary>
    ///     收取单块无人岛耕地
    /// </summary>
    public static void Collect(uint farmIndex, bool dismissAfterCollect) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIFarmCollectSingle, farmIndex, dismissAfterCollect ? 1U : 0U);
}
