using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class EnterMateriaAttachStateCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.EnterMateriaAttachState;

    /// <summary>
    ///     进入镶嵌魔晶石状态
    /// </summary>
    public void Enter(uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, itemID);
}