using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SendDuelCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.SendDuel;

    /// <summary>
    ///     发起决斗
    /// </summary>
    public void Send(GameObjectId targetGameObjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)targetGameObjectID);
}
