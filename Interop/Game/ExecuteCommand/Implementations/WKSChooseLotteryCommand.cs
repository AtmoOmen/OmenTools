using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSChooseLotteryCommand : ExecuteCommandBase
{
    /// <summary>
    ///     宇宙好运道选择转盘
    /// </summary>
    public static void Choose(CurrencyKind currencyKind, WheelKind wheelKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WKSChooseLottery, (uint)currencyKind, (uint)wheelKind);

    public enum CurrencyKind : uint
    {
        LunarCredit   = 0,
        PhaennaCredit = 1,
        OccultCredit  = 2
    }

    public enum WheelKind : uint
    {
        Left  = 0,
        Right = 1
    }
}
