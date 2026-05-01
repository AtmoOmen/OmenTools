using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class MJIFarmCollectAllCommand : ExecuteCommandBase
{
    /// <summary>
    ///     收取全部无人岛耕地
    /// </summary>
    public static void Collect() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIFarmCollectAll, *(uint*)MJIManager.Instance()->GranariesState);
}
