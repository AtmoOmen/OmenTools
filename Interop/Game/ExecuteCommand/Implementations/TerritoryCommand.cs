using FFXIVClientStructs.FFXIV.Client.Game.UI;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TerritoryCommand : ExecuteCommandBase
{
    /// <summary>
    ///     执行区域变更
    /// </summary>
    /// <param name="warpType">区域变更类型</param>
    public static void Transport
    (
        WarpType warpType
    ) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.NotifyWarpComplete, 0, (uint)warpType);

    /// <summary>
    ///     区域变更完成
    /// </summary>
    public static void NotifyTerritoryReady() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.NotifyTerritoryReady);
}
