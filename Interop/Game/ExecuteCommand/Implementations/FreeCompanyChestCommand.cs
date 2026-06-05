using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FreeCompanyChestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     向部队储物柜存入金币
    /// </summary>
    public static void DepositGil(uint amount) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DepositFreeCompanyChestGil, amount);

    /// <summary>
    ///     从部队储物柜取出金币
    /// </summary>
    public static void WithdrawGil(uint amount) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WithdrawFreeCompanyChestGil, amount);

    /// <summary>
    ///     请求部队储物柜操作历史记录
    /// </summary>
    public static void RequestLog() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestFreeCompanyChestLog);
}
