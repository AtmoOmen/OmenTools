using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class SaveHousingGuestAccessCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.SaveHousingGuestAccess;

    /// <summary>
    ///     保存当前房屋访客权限设置
    /// </summary>
    public void Save(bool allowTeleport, bool allowEnter, bool isIndoor = true)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID(isIndoor);
        var flags = (allowTeleport ? 1U : 0U) | (allowEnter ? 65536U : 0U);
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, houseIDHigh, houseID, flags);
    }

    private static (uint High, uint Low) GetCurrentHouseID(bool isIndoor)
    {
        if (isIndoor)
        {
            var territory = HousingManager.Instance()->IndoorTerritory;
            var high = *(long*)((nint)territory + 38560) >> 32;
            return ((uint)high, (uint)territory->HouseId);
        }

        var outdoorTerritory = HousingManager.Instance()->OutdoorTerritory;
        var outdoorHigh = *(long*)((nint)outdoorTerritory + 38560) >> 32;
        return ((uint)outdoorHigh, (uint)outdoorTerritory->HouseId);
    }
}