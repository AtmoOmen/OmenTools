using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJICollectAnimalLeavingsCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJICollectAnimalLeavings;

    /// <summary>
    ///     收集无人岛牧场动物产物
    /// </summary>
    public void Collect(uint animalIndex, uint collectFlag) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, animalIndex, collectFlag);
}
