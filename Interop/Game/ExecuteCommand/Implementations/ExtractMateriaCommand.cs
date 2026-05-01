using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

// TODO: 需要确定是否可用
public sealed class ExtractMateriaCommand : ExecuteCommandBase
{
    /// <summary>
    ///     精制魔晶石
    /// </summary>
    public static void Extract(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ExtractMateria, (uint)inventoryType, inventorySlot);
}
