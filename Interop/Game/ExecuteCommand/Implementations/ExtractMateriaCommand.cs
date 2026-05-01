using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ExtractMateriaCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.ExtractMateria;

    /// <summary>
    ///     精制魔晶石
    /// </summary>
    public void Extract(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)inventoryType, inventorySlot);
}
