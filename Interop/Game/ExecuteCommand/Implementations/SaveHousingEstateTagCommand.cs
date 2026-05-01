using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed unsafe class SaveHousingEstateTagCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.SaveHousingEstateTag;

    /// <summary>
    ///     保存当前房屋宣传设置
    /// </summary>
    public void Save(uint tagFlags, bool isIndoor = true)
    {
        var (houseIDHigh, houseID) = GetCurrentHouseID(isIndoor);
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, houseIDHigh, houseID, tagFlags);
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
