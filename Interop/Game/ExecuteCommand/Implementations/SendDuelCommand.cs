using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SendDuelCommand : ExecuteCommandBase
{
    /// <summary>
    ///     发起决斗
    /// </summary>
    public static void Send(GameObjectId targetGameObjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendDuel, (uint)targetGameObjectID);
}
