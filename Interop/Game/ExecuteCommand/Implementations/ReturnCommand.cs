using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ReturnCommand : ExecuteCommandBase
{
    /// <summary>
    ///     若当前种族不是拉拉菲尔族, 则返回至最近安全点
    /// </summary>
    public static void IfNotLalafell() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ReturnToSafePointIfNotLalafell);

    /// <summary>
    ///     立即返回至返回点
    /// </summary>
    public static void Instant() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ReturnIfNotLalafell);
}
