using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SelectPVPRoleActionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     选择 PVP 职能技能
    /// </summary>
    public static void Select(uint roleActionIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SelectPVPRoleAction, roleActionIndex);
}
