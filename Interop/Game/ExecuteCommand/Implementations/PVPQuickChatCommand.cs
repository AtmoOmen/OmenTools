using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class PVPQuickChatCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.PVPQuickChat;

    /// <summary>
    ///     发送 PVP 快捷发言
    /// </summary>
    public void Send(uint quickChatRowID, uint param1 = 0, uint param2 = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, quickChatRowID, param1, param2);
}