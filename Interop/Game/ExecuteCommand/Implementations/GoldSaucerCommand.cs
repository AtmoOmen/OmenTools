using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class GoldSaucerCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求金碟游乐场面板整体信息
    /// </summary>
    public static void RequestGeneral() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGoldSaucerGeneral);

    /// <summary>
    ///     请求金碟游乐场面板陆行鸟信息
    /// </summary>
    public static void RequestChocobo() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGoldSaucerChocobo);

    /// <summary>
    ///     请求金碟游乐场面板萌宠之王信息
    /// </summary>
    public static void RequestLordOfVerminion() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGoldSaucerVerminion);

    /// <summary>
    ///     萌宠之王小宠物编队确认
    /// </summary>
    public static void ConfirmVerminionPalette() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ConfirmVerminionPalette);

    /// <summary>
    ///     请求金碟游乐场面板多玛方城战信息
    /// </summary>
    public static void RequestMahjong() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGoldSaucerMahjong);
}
