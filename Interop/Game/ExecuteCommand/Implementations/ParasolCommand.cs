using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ParasolCommand : ExecuteCommandBase
{
    /// <summary>
    ///     因执行其他动作或不满足条件而强行收起时尚配饰
    /// </summary>
    public static void WithdrawForced() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WithdrawParasolForced);

    /// <summary>
    ///     主动收起时尚配饰
    /// </summary>
    public static void Withdraw() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WithdrawParasol);

    /// <summary>
    ///     根据当前情况使用或收回时尚配饰
    /// </summary>
    public static void Update(uint parasolID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.UpdateParasolState, parasolID);

    /// <summary>
    ///     设置时尚配饰
    /// </summary>
    public static void Set(uint parasolID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetParasolToAutoUse, parasolID);

    /// <summary>
    ///     取消选择时尚配饰
    /// </summary>
    public static void Clear() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetParasolToAutoUse);
}
