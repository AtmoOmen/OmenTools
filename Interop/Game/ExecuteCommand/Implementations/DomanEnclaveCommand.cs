using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class DomanEnclaveCommand : ExecuteCommandBase
{
    /// <summary>
    ///     买回多玛飞地支援物资
    /// </summary>
    public static void Buyback(uint itemIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.BuybackReconstrcutionItem, itemIndex);

    /// <summary>
    ///     请求支援物资退还箱物资数据
    /// </summary>
    public static void RequestBuyBack() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestReconstrcutionBuyBack);

    /// <summary>
    ///     完成请求支援物资退还箱物资数据
    /// </summary>
    public static void FinishRequestBuyBack() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishRequestReconstrcutionBuyBack);

    /// <summary>
    ///     请求重建多玛相关数据
    /// </summary>
    public static void Request() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDomanEnclave);
}
