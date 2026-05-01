using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIGranaryViewDestinationsCommand : ExecuteCommandBase
{
    /// <summary>
    ///     查看无人岛屯货仓库探索目的地
    /// </summary>
    public static void View(uint granaryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIGranaryViewDestinations, granaryIndex);
}
