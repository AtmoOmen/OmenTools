using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class DismountCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Dismount;

    /// <summary>
    ///     下坐骑
    /// </summary>
    public void Execute(bool enqueue = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, enqueue ? 1U : 0U);
}
