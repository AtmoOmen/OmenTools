using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AutoAttackCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.AutoAttack;

    /// <summary>
    ///     设置自动攻击状态
    /// </summary>
    public void Set(bool isEnabled, uint targetObjectID, bool isManual = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, isEnabled ? 1U : 0U, targetObjectID, isManual ? 1U : 0U);
}
