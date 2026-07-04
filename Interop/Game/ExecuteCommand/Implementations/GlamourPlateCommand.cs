using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class GlamourPlateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     应用投影模板
    /// </summary>
    public static void ApplyPlate(uint plateIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ApplyGlamourPlate, plateIndex);

    /// <summary>
    ///    应用投影到指定装备上
    /// </summary>
    public static void Apply(uint glamourIndex, InventoryType type, ushort slot) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ApplyGlamourPlate, glamourIndex, (uint)type, slot);

    /// <summary>
    ///     请求投影模板数据
    /// </summary>
    public static void Request() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGlamourPlate);

    /// <summary>
    ///     为装备解除投影
    /// </summary>
    public static void DispellItems(uint dispellItemsSelectedBitmask) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DispellGlamours, dispellItemsSelectedBitmask);

    /// <summary>
    ///     进入投影模板选择状态
    /// </summary>
    public static void Enter() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGlamourPlate, 1, 1);

    /// <summary>
    ///     退出投影模板选择状态
    /// </summary>
    public static void Exit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGlamourPlate, 0, 1);

    /// <summary>
    ///     进入/退出投影模板选择状态
    /// </summary>
    public static void Toggle(bool isEnter) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGlamourPlate, isEnter ? 1U : 0, 1);
}
