using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairSubmarinePartCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RepairSubmarinePart;

    /// <summary>
    ///     修理潜水艇部件
    /// </summary>
    public void Repair(uint submarineIndex, uint partIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, submarineIndex, partIndex);
}
