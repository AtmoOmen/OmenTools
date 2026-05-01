using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BuddyActionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.BuddyAction;

    /// <summary>
    ///     使用陆行鸟技能
    /// </summary>
    /// <seealso cref="BuddyAction" />
    public void Execute(ActionType actionType) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)actionType);

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
