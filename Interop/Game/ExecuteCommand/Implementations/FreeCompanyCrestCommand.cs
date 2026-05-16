using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FreeCompanyCrestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     为装备贴上或取下部队队徽
    /// </summary>
    public static void Apply(InventoryType inventoryType, uint inventorySlot, bool isAttach) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.FreeCompanyCrestDecal,
            (uint)inventoryType,
            inventorySlot,
            isAttach ? 1U : 0U
        );

    /// <summary>
    ///     批量为装备中装备贴上或取下部队队徽
    /// </summary>
    public static void ApplyBatchEquipped(bool isAttach) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FreeCompanyCrestDecalBatchEquipped, isAttach ? 1U : 0U);

    /// <summary>
    ///     批量为指定范围装备贴上或取下部队队徽
    /// </summary>
    public static void ApplyBatch(BatchTarget target, bool isAttach) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FreeCompanyCrestDecalBatch, (uint)target, isAttach ? 1U : 0U);

    public enum BatchTarget : uint
    {
        Armory    = 5,
        Inventory = 6
    }
}
