using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSEndLotteryCommand : ExecuteCommandBase
{
    /// <summary>
    ///     宇宙好运道结束抽奖
    /// </summary>
    public static void End(CurrencyKind currencyKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WKSEndLottery, (uint)currencyKind);

    public enum CurrencyKind : uint
    {
        LunarCredit   = 0,
        PhaennaCredit = 1,
        OccultCredit  = 2
    }
}
