using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateLevelSyncCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.FateLevelSync;

    /// <summary>
    ///     为临危受命设置等级同步状态
    /// </summary>
    public void Set(uint fateID, bool isLevelSynced) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, fateID, isLevelSynced ? 1U : 0U);
}
