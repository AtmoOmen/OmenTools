using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CancelRepairRequestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     取消修理委托
    /// </summary>
    public static void Cancel(uint targetEntityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.CancelRepairRequest, targetEntityID);
}
