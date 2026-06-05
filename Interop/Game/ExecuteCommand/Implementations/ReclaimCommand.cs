using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ReclaimCommand : ExecuteCommandBase
{
    /// <summary>
    ///     清空回收仓库通知
    /// </summary>
    public static void Clear() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ClearReclaimNotification);

    /// <summary>
    ///     取回全部 1.0 遗产物品或临时保管家具
    /// </summary>
    public static void ReclaimItems() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ReclaimItems);
}
