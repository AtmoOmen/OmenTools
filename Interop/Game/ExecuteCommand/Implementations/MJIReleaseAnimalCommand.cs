using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIReleaseAnimalCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIReleaseAnimal;

    /// <summary>
    ///     放生无人岛牧场动物
    /// </summary>
    public void Release(uint animalIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, animalIndex);
}