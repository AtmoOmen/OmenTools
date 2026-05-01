using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MateriaCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入镶嵌魔晶石状态
    /// </summary>
    public static void EnterAttachState(uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EnterMateriaAttachState, itemID);

    /// <summary>
    ///     离开镶嵌魔晶石状态
    /// </summary>
    public static void LeaveAttachState() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.LeaveMateriaAttachState);

    // TODO: 需要确定是否可用
    /// <summary>
    ///     精制魔晶石
    /// </summary>
    public static void Extract(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ExtractMateria, (uint)inventoryType, inventorySlot);
}
