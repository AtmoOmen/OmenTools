using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class EnterMateriaAttachStateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入镶嵌魔晶石状态
    /// </summary>
    public static void Enter(uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EnterMateriaAttachState, itemID);
}
