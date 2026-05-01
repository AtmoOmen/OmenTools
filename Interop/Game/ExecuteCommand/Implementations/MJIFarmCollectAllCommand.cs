using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class MJIFarmCollectAllCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIFarmCollectAll;

    /// <summary>
    ///     收取全部无人岛耕地
    /// </summary>
    public void Collect() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, *(uint*)MJIManager.Instance()->GranariesState);
}