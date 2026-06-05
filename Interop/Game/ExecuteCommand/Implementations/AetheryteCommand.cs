using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AetheryteCommand : ExecuteCommandBase
{
    /// <summary>
    ///     移除收藏夹内的以太之光
    /// </summary>
    public static void RemoveFavorite(uint aetheryteID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RemoveFavoriteAetheryte, aetheryteID);

    /// <summary>
    ///     移除免费传送点
    /// </summary>
    public static void RemoveFree() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RemoveFreeAetheryte);

    /// <summary>
    ///     移除 PlayStation Plus 会员可设置的免费传送点
    /// </summary>
    public static void RemovePSPlusFree() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RemoveFreeAetherytePSPlus);

    /// <summary>
    ///     移除 Nintendo Switch Online 会员可设置的免费传送点
    /// </summary>
    public static void RemoveNSOFree() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RemoveFreeAetheryteNSO);
}
