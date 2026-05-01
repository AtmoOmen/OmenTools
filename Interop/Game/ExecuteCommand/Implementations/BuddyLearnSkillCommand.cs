using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BuddyLearnSkillCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.BuddyLearnSkill;

    /// <summary>
    ///     陆行鸟学习技能
    /// </summary>
    public void Learn(Category category, uint level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1U);
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (level - 1) * 3 + (uint)category);
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
}
