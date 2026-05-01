using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class DismountCommand : ExecuteCommandBase
{
    /// <summary>
    ///     下坐骑
    /// </summary>
    public static void Execute(bool enqueue = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Dismount, enqueue ? 1U : 0U);
}
