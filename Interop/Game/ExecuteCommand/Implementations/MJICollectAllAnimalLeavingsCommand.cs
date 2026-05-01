using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class MJICollectAllAnimalLeavingsCommand : ExecuteCommandBase
{
    /// <summary>
    ///     收取无人岛牧场全部动物产物
    /// </summary>
    public static void Collect() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJICollectAllAnimalLeavings, (uint)MJIManager.Instance()->PastureHandler->AvailableMammetLeavings.Count);
}
