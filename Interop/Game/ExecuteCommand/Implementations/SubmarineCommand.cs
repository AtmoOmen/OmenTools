using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SubmarineCommand : ExecuteCommandBase
{
    /// <summary>
    ///     修理潜水艇部件
    /// </summary>
    public static void RepairPart(uint submarineIndex, uint partIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairSubmarinePart, submarineIndex, partIndex);
}
