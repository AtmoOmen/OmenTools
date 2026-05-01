using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BuddyCommand : ExecuteCommandBase
{
    /// <summary>
    ///     使用陆行鸟技能
    /// </summary>
    /// <seealso cref="BuddyAction" />
    public static void UseAction(ActionType actionType) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.BuddyAction, (uint)actionType);

    /// <summary>
    ///     设置陆行鸟装甲
    /// </summary>
    /// <seealso cref="BuddyEquip" />
    public static void Equip(Part part, uint buddyEquipRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.BuddyEquip, (uint)part, buddyEquipRowID);

    /// <summary>
    ///     卸下陆行鸟装甲
    /// </summary>
    public static void Unequip(Part part) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.BuddyEquip, (uint)part);

    /// <summary>
    ///     陆行鸟学习技能
    /// </summary>
    public static void LearnSkill(Category category, uint level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1U);
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.BuddyLearnSkill, (level - 1) * 3 + (uint)category);
    }

    public enum Category : uint
    {
        /// <summary>
        ///     防护技能
        /// </summary>
        Defender,

        /// <summary>
        ///     进攻技能
        /// </summary>
        Attacker,

        /// <summary>
        ///     治疗技能
        /// </summary>
        Healer
    }

    public enum Part : uint
    {
        Head = 0,
        Body = 1,
        Legs = 2
    }

    public enum ActionType : uint
    {
        /// <summary>
        ///     离开
        /// </summary>
        Withdraw = 2,

        /// <summary>
        ///     跟随
        /// </summary>
        Follow,

        /// <summary>
        ///     自由战术
        /// </summary>
        FreeStance,

        /// <summary>
        ///     防护战术
        /// </summary>
        DefenderStance,

        /// <summary>
        ///     进攻战术
        /// </summary>
        AttackerStance,

        /// <summary>
        ///     治疗战术
        /// </summary>
        HealerStance
    }
}
