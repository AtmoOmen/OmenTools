using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SendRepairRequestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     发送修理委托
    /// </summary>
    public static void Send(uint targetEntityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendRepairRequest, targetEntityID);
}
