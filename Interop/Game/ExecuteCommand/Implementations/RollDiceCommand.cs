using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RollDiceCommand : ExecuteCommandBase
{
    /// <summary>
    ///     掷骰子
    /// </summary>
    public static void Roll(uint maxValue) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RollDice, 0, maxValue);
}
