using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSStartMissionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.WKSStartMission;

    /// <summary>
    ///     宇宙探索接取任务
    /// </summary>
    public void Start(uint missionUnitID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, missionUnitID);
}