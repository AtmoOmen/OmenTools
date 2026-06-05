using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WorkshopCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求飞空艇数据
    /// </summary>
    public static void RequestAirship() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestAirship);

    /// <summary>
    ///     刷新部队合建物品交纳信息
    /// </summary>
    public static void RefreshFCMaterialDelivery() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestCompanyProject);

    /// <summary>
    ///     请求潜水艇完成情况信息
    /// </summary>
    public static void RequestSubmarine() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestSubmarine);
}
