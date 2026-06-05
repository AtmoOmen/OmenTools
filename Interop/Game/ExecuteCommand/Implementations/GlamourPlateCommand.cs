using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class GlamourPlateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     应用投影模板
    /// </summary>
    public static void Apply(uint plateIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ApplyGlamourPlate, plateIndex);

    /// <summary>
    ///     从投影台应用幻化模板
    /// </summary>
    public static void ApplyFromPrismBox(uint plateIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ApplyGlamourPlateFromPrismBox, plateIndex);

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
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ToggleGlamourPlateState, 1, 1);

    /// <summary>
    ///     退出投影模板选择状态
    /// </summary>
    public static void Exit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ToggleGlamourPlateState, 0, 1);

    /// <summary>
    ///     进入/退出投影模板选择状态
    /// </summary>
    public static void Toggle(bool isEnter) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ToggleGlamourPlateState, isEnter ? 1U : 0, 1);
}
