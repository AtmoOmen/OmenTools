using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSEndLotteryCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.WKSEndLottery;

    /// <summary>
    ///     宇宙好运道结束抽奖
    /// </summary>
    public void End(CurrencyKind currencyKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)currencyKind);

    public enum CurrencyKind : uint
    {
        LunarCredit   = 0,
        PhaennaCredit = 1,
        OccultCredit  = 2
    }
}
