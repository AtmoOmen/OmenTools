using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SummonPetCommand : ExecuteCommandBase
{
    /// <summary>
    ///     召唤宠物
    /// </summary>
    public static void Summon(uint petID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SummonPet, petID);
}
