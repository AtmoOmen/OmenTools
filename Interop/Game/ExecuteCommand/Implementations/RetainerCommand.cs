using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RetainerCommand : ExecuteCommandBase
{
    /// <summary>
    ///     设置当前雇员市场出售物品价格
    /// </summary>
    public static void SetMarketPrice(uint slot, uint price) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetRetainerMarketPrice, slot, price);

    /// <summary>
    ///     请求雇员探险时间信息
    /// </summary>
    public static void RequestTime() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestRetainerVentureTime);

    /// <summary>
    ///     返回雇员
    /// </summary>
    public static void Return() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Retainer, 0, 3);

    /// <summary>
    ///     刷新雇员信息
    /// </summary>
    public static void Refresh() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Retainer, 0, 4);

    /// <summary>
    ///     委托雇员探险
    /// </summary>
    public static void AssignVenture(uint retainerTaskID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Retainer, 0, 5, retainerTaskID);

    /// <summary>
    ///     撤销雇员探险
    /// </summary>
    public static void CancelVenture() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Retainer, 0, 6);

    /// <summary>
    ///     为雇员武具投影
    /// </summary>
    public static void CastGlamour
    (
        InventoryType targetInventoryType,
        uint          targetInventorySlot,
        InventoryType sourceInventoryType,
        uint          sourceInventorySlot
    ) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.CastRetainerGlamour,
            (uint)targetInventoryType,
            targetInventorySlot,
            (uint)sourceInventoryType,
            sourceInventorySlot
        );
}
