using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIReleaseMinionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIReleaseMinion;

    /// <summary>
    ///     在无人岛放养宠物
    /// </summary>
    public void Release(uint minionID, uint areaIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, minionID, areaIndex);
}
