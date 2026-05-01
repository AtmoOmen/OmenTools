using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIReleaseAnimalCommand : ExecuteCommandBase
{
    /// <summary>
    ///     放生无人岛牧场动物
    /// </summary>
    public static void Release(uint animalIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIReleaseAnimal, animalIndex);
}
