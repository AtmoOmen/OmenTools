using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class MJICollectAllAnimalLeavingsCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJICollectAllAnimalLeavings;

    /// <summary>
    ///     收取无人岛牧场全部动物产物
    /// </summary>
    public void Collect() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)MJIManager.Instance()->PastureHandler->AvailableMammetLeavings.Count);
}