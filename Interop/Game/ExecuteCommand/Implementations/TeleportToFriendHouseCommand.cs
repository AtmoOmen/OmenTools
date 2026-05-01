using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TeleportToFriendHouseCommand : ExecuteCommandBase
{
    /// <summary>
    ///     传送至好友房屋
    /// </summary>
    public static void Teleport(uint param1, uint param2, FriendHouseAetheryteKind aetheryteKind, uint aetheryteSubID = 1) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.TeleportToFriendHouse, param1, param2, (uint)aetheryteKind, aetheryteSubID);

    public enum FriendHouseAetheryteKind : uint
    {
        FreeCompanyHouse = 57,
        PrivateHouse     = 61
    }
}
