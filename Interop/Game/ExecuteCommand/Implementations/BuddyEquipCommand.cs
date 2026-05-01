using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BuddyEquipCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.BuddyEquip;

    /// <summary>
    ///     设置陆行鸟装甲
    /// </summary>
    public void Equip(Part part, uint buddyEquipRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)part, buddyEquipRowID);

    /// <summary>
    ///     卸下陆行鸟装甲
    /// </summary>
    public void Unequip(Part part) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)part);

    public enum Part : uint
    {
        Head = 0,
        Body = 1,
        Legs = 2
    }
}