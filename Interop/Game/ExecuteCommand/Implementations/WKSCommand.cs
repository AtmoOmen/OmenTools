using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSCommand : ExecuteCommandBase
{
    /// <summary>
    ///     变更宇宙探索模式
    /// </summary>
    public static void ChangeMode(uint modeIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetWKSMode, modeIndex);

    /// <summary>
    ///     宇宙探索接取任务
    /// </summary>
    public static void StartMission(uint missionUnitID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AcceptWKSMission, missionUnitID);

    /// <summary>
    ///     宇宙探索完成任务
    /// </summary>
    public static void CompleteMission() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishWKSMission);

    /// <summary>
    ///     宇宙探索放弃任务
    /// </summary>
    public static void AbandonMission() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AbandonWKSMission);

    /// <summary>
    ///     宇宙好运道开始抽奖
    /// </summary>
    public static void StartLottery(CurrencyKind currencyKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StartWKSLottery, (uint)currencyKind);

    /// <summary>
    ///     宇宙好运道选择转盘
    /// </summary>
    public static void ChooseLottery(CurrencyKind currencyKind, WheelKind wheelKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ChooseWKSLotteryType, (uint)currencyKind, (uint)wheelKind);

    /// <summary>
    ///     宇宙好运道结束抽奖
    /// </summary>
    public static void EndLottery(CurrencyKind currencyKind) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishWKSLottery, (uint)currencyKind);

    /// <summary>
    ///     宇宙探索请求机甲数据
    /// </summary>
    public static void RequestMecha(uint wksMechaEventDataRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestWKSMecha, wksMechaEventDataRowID);

    /// <summary>
    ///     宇宙探索请求当前未开始的机甲数据
    /// </summary>
    public static void RequestInactiveMecha() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestWKSMecha);

    /// <summary>
    ///     宇宙探索结束交互
    /// </summary>
    public static void EndInteraction() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishWKSInteraction3401);

    /// <summary>
    ///     宇宙探索结束交互
    /// </summary>
    public static void EndInteractionAlternate() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishWKSInteraction3402);

    /// <summary>
    ///     宇宙探索请求探索成果数据
    /// </summary>
    public static void RequestSuccesses() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestWKSSuccesses);

    public enum CurrencyKind : uint
    {
        LunarCredit   = 0,
        PhaennaCredit = 1,
        OizysCredit   = 2
    }

    public enum WheelKind : uint
    {
        Left  = 0,
        Right = 1
    }
}
