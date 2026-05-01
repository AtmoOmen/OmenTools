using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSChooseLotteryCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.WKSChooseLottery;

    /// <summary>
    ///     宇宙好运道选择转盘
    /// </summary>
    public void Choose(CurrencyKind currencyKind, WheelKind wheelKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)currencyKind, (uint)wheelKind);

    public enum CurrencyKind : uint
    {
        LunarCredit = 0,
        PhaennaCredit = 1,
        OccultCredit = 2
    }

    public enum WheelKind : uint
    {
        Left = 0,
        Right = 1
    }
}