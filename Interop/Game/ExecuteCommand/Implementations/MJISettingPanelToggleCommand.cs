using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJISettingPanelToggleCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJISettingPanelToggle;

    /// <summary>
    ///     设置无人岛设置面板开关状态
    /// </summary>
    public void Set(bool isOpen) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, isOpen ? 1U : 0U);
}
