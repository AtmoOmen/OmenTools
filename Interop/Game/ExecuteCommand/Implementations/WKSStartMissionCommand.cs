using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSStartMissionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     宇宙探索接取任务
    /// </summary>
    public static void Start(uint missionUnitID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WKSStartMission, missionUnitID);
}
