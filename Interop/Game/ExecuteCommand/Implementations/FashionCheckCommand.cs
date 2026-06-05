using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FashionCheckCommand : ExecuteCommandBase
{
    /// <summary>
    ///     获取时尚品鉴每周参与奖励
    /// </summary>
    public static void GetEntryReward() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.CliamFashionCheckEntryReward);

    /// <summary>
    ///     获取时尚品鉴每周额外奖励
    /// </summary>
    public static void GetBonusReward() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ClaimFashionCheckBonusReward);

    /// <summary>
    ///     时尚品鉴新增装备条目与额外奖励
    /// </summary>
    public static void AddEntryAndBonusReward() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ClaimFashionCheckNewGearReward);
}
