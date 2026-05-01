using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ViewHouseDetailCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.ViewHouseDetail;

    /// <summary>
    ///     查看房屋详情
    /// </summary>
    public void View(uint territoryType, uint wardID, uint houseID, uint apartmentRoomIndex = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, territoryType, wardID * 256 + houseID, apartmentRoomIndex);
}
