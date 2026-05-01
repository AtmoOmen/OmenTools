using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSRequestMechaCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.WKSRequestMecha;

    /// <summary>
    ///     宇宙探索请求机甲数据
    /// </summary>
    public void Request(uint wksMechaEventDataRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, wksMechaEventDataRowID);

    /// <summary>
    ///     宇宙探索请求当前未开始的机甲数据
    /// </summary>
    public void RequestInactive() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag);
}
