using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ViewHouseDetailCommand : ExecuteCommandBase
{
    /// <summary>
    ///     查看房屋详情
    /// </summary>
    public static void View(uint territoryType, uint wardID, uint houseID, uint apartmentRoomIndex = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ViewHouseDetail, territoryType, wardID * 256 + houseID, apartmentRoomIndex);
}
