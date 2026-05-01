using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSStartLotteryCommand : ExecuteCommandBase
{
    /// <summary>
    ///     宇宙好运道开始抽奖
    /// </summary>
    public static void Start(CurrencyKind currencyKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WKSStartLottery, (uint)currencyKind);

    public enum CurrencyKind : uint
    {
        LunarCredit   = 0,
        PhaennaCredit = 1,
        OccultCredit  = 2
    }
}
