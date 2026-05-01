using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIFarmDismissCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIFarmDismiss;

    /// <summary>
    ///     取消托管单块无人岛耕地
    /// </summary>
    public void Dismiss(uint farmIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, farmIndex);
}
