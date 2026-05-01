using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class PVPQuickChatCommand : ExecuteCommandBase
{
    /// <summary>
    ///     发送 PVP 快捷发言
    /// </summary>
    public static void Send(uint quickChatRowID, uint param1 = 0, uint param2 = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.PVPQuickChat, quickChatRowID, param1, param2);
}
