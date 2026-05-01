using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SelectPVPRoleActionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.SelectPVPRoleAction;

    /// <summary>
    ///     选择 PVP 职能技能
    /// </summary>
    public void Select(uint roleActionIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, roleActionIndex);
}