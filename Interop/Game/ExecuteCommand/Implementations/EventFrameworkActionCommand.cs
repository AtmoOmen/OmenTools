using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class EventFrameworkActionCommand : ExecuteCommandBase
{
    // TODO: 之后逆向完再看看
    /*/// <summary>
    ///     执行分解
    /// </summary>
    public static void Desynthesize() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735552);

    /// <summary>
    ///     执行回收魔晶石
    /// </summary>
    public static void RetrieveMateria() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735553);

    /// <summary>
    ///     执行精选
    /// </summary>
    public static void AetherialReduction() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735554);*/

    /// <summary>
    ///     修理装备中物品
    /// </summary>
    public static void RepairEquippedItems() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735555, 2, 1000);

    /// <summary>
    ///     修理分页物品
    /// </summary>
    public static void RepairPage(RepairPageCategory category) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735555, 3, (uint)category);

    /// <summary>
    ///     修理单独物品
    /// </summary>
    public static void RepairItem(InventoryType inventoryType, uint inventorySlot, uint itemID, bool isHQ = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.EventFrameworkAction,
            3735555,
            inventorySlot << 16 | 1,
            (uint)inventoryType,
            itemID + (isHQ ? 1000000U : 0U)
        );

    public enum RepairPageCategory : uint
    {
        /// <summary>
        ///     主手/副手
        /// </summary>
        MainOffHands = 0,

        /// <summary>
        ///     头部/身体/手臂
        /// </summary>
        HeadBodyArms = 1,

        /// <summary>
        ///     腿部/脚部
        /// </summary>
        LegsFeet = 2,

        /// <summary>
        ///     耳部/颈部
        /// </summary>
        EarsNeck = 3,

        /// <summary>
        ///     腕部/戒指
        /// </summary>
        WristsRings = 5,

        /// <summary>
        ///     物品
        /// </summary>
        Inventory = 6
    }
}
