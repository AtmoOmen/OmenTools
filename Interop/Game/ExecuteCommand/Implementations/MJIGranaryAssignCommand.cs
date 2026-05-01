using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIGranaryAssignCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIGranaryAssign;

    /// <summary>
    ///     无人岛屯货仓库派遣探险
    /// </summary>
    public void Assign(uint granaryIndex, uint destinationIndex, uint explorationDays) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, granaryIndex, destinationIndex, explorationDays);
}
