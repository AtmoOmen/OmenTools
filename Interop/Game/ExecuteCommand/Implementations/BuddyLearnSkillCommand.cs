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
    public void Learn(uint skillIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, skillIndex);
}