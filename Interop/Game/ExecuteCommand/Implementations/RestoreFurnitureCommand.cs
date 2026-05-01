using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class RestoreFurnitureCommand : ExecuteCommandBase
{
    /// <summary>
    ///     从当前房屋中取回指定家具
    /// </summary>
    public static void Restore(InventoryType inventoryType, uint inventorySlot, bool isIndoor = true, bool storePlacedFurniture = false)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID(isIndoor);
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.RestoreFurniture,
            houseIDHigh,
            houseID,
            (uint)inventoryType,
            storePlacedFurniture ? inventorySlot + 65536U : inventorySlot
        );
    }

    private static (uint High, uint Low) GetCurrentHouseID(bool isIndoor)
    {
        if (isIndoor)
        {
            var territory = HousingManager.Instance()->IndoorTerritory;
            var high      = *(long*)((nint)territory + 38560) >> 32;
            return ((uint)high, (uint)territory->HouseId);
        }

        var outdoorTerritory = HousingManager.Instance()->OutdoorTerritory;
        var outdoorHigh      = *(long*)((nint)outdoorTerritory + 38560) >> 32;
        return ((uint)outdoorHigh, (uint)outdoorTerritory->HouseId);
    }
}
