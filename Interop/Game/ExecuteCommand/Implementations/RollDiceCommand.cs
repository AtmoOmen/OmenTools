using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RollDiceCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RollDice;

    /// <summary>
    ///     掷骰子
    /// </summary>
    public void Roll(uint maxValue) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, maxValue);
}