using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIEntrustAnimalCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIEntrustAnimal;

    /// <summary>
    ///     托管无人岛牧场动物
    /// </summary>
    public void Entrust(uint animalIndex, uint feeditemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, animalIndex, feeditemID);
}