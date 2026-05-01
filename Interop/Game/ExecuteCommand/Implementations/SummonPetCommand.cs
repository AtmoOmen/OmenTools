using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SummonPetCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.SummonPet;

    /// <summary>
    ///     召唤宠物
    /// </summary>
    public void Summon(uint petID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, petID);
}
