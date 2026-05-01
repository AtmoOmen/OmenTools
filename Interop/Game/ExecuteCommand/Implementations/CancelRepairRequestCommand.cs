using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CancelRepairRequestCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.CancelRepairRequest;

    /// <summary>
    ///     取消修理委托
    /// </summary>
    public void Cancel(uint targetEntityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, targetEntityID);
}