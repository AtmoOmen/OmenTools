using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SendRepairRequestCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.SendRepairRequest;

    /// <summary>
    ///     发送修理委托
    /// </summary>
    public void Send(uint targetEntityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, targetEntityID);
}
