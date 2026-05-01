using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSRequestMechaCommand : ExecuteCommandBase
{
    /// <summary>
    ///     宇宙探索请求机甲数据
    /// </summary>
    public static void Request(uint wksMechaEventDataRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WKSRequestMecha, wksMechaEventDataRowID);

    /// <summary>
    ///     宇宙探索请求当前未开始的机甲数据
    /// </summary>
    public static void RequestInactive() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WKSRequestMecha);
}
