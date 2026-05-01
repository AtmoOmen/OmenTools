using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIGranaryViewDestinationsCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIGranaryViewDestinations;

    /// <summary>
    ///     查看无人岛屯货仓库探索目的地
    /// </summary>
    public void View(uint granaryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, granaryIndex);
}
