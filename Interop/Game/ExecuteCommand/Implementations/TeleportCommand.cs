using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TeleportCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Teleport;

    /// <summary>
    ///     传送至指定以太之光
    /// </summary>
    public void Teleport(uint aetheryteID, bool useTicket, uint aetheryteSubID = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, aetheryteID, useTicket ? 1U : 0U, aetheryteSubID);
}