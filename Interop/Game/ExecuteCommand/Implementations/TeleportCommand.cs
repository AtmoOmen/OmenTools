using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TeleportCommand : ExecuteCommandBase
{
    /// <summary>
    ///     传送至指定以太之光
    /// </summary>
    public static void Teleport(uint aetheryteID, uint aetheryteSubID, bool useTicket) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Teleport, aetheryteID, useTicket ? 1U : 0U, aetheryteSubID);

    /// <summary>
    ///     接受传送邀请
    /// </summary>
    public static void AcceptOffer() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AcceptTeleportOffer);

    /// <summary>
    ///     取消传送
    /// </summary>
    public static void Cancel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.CancelTeleport);
}
