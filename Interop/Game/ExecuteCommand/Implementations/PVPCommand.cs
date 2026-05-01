using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class PVPCommand : ExecuteCommandBase
{
    /// <summary>
    ///     选择 PVP 职能技能
    /// </summary>
    public static void SelectRoleAction(uint roleActionIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SelectPVPRoleAction, roleActionIndex);

    /// <summary>
    ///     发起决斗
    /// </summary>
    public static void SendDuel(GameObjectId targetGameObjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendDuel, (uint)targetGameObjectID);

    /// <summary>
    ///     确认决斗申请
    /// </summary>
    public static void ConfirmDuel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDuel);

    /// <summary>
    ///     取消决斗申请
    /// </summary>
    public static void CancelDuel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDuel, 1);

    /// <summary>
    ///     强制取消决斗申请
    /// </summary>
    public static void ForceCancelDuel() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestDuel, 4);

    /// <summary>
    ///     领取战利水晶
    /// </summary>
    public static void CollectTrophyCrystal(Season season = Season.Current) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.CollectTrophyCrystal, (uint)season);

    /// <summary>
    ///     发送 PVP 快捷发言
    /// </summary>
    public static void QuickChat(uint quickChatRowID, uint param1 = 0, uint param2 = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.PVPQuickChat, quickChatRowID, param1, param2);

    public enum Season : uint
    {
        Current  = 0,
        Previous = 1
    }
}
