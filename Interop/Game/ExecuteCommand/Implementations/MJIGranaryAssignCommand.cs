using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIGranaryAssignCommand : ExecuteCommandBase
{
    /// <summary>
    ///     无人岛屯货仓库派遣探险
    /// </summary>
    public static void Assign(uint granaryIndex, uint destinationIndex, uint explorationDays) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIGranaryAssign, granaryIndex, destinationIndex, explorationDays);
}
