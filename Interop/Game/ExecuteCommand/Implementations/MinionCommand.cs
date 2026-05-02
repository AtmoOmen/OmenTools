using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MinionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     召唤宠物
    /// </summary>
    public static void Summon(uint minionID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SummonMinion, minionID);
    
    /// <summary>
    ///     收回宠物
    /// </summary>
    public static void Withdraw() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WithdrawMinion);
}
