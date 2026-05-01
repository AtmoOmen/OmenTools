using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIReleaseMinionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     在无人岛放养宠物
    /// </summary>
    public static void Release(uint minionID, uint areaIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIReleaseMinion, minionID, areaIndex);
}
