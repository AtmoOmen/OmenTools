using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MateriaCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入镶嵌魔晶石状态
    /// </summary>
    public static void EnterAttachState(uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EnterMateriaAttachState, itemID);

    /// <summary>
    ///     完成镶嵌魔晶石状态
    /// </summary>
    public static void FinishAttachState() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishMateriaAttach);

    /// <summary>
    ///     离开镶嵌魔晶石状态
    /// </summary>
    public static void LeaveAttachState() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.LeaveMateriaAttachState);

    /// <summary>
    ///     请求帮助镶嵌魔晶石
    /// </summary>
    public static void SendAttachRequest(uint targetEntityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendMateriaAttachRequest, targetEntityID);

    /// <summary>
    ///     因职业变更自动取消魔晶石镶嵌委托
    /// </summary>
    public static void CancelAttachRequest() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.CancelMateriaAttachRequestForced);

    // TODO: 需要确定是否可用
    /// <summary>
    ///     精制魔晶石
    /// </summary>
    public static void Extract(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ExtractMateria, (uint)inventoryType, inventorySlot);

    /// <summary>
    ///     回收魔晶石
    /// </summary>
    public static unsafe void Retrieve(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.EventFrameworkAction,
            3735553,
            (uint)inventoryType,
            inventorySlot,
            InventoryManager.Instance()->GetInventorySlot(inventoryType, (int)inventorySlot)->GetItemId()
        );
}
