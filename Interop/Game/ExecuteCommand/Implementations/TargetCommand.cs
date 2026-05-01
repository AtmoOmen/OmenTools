using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TargetCommand : ExecuteCommandBase
{
    /// <summary>
    ///     选中目标
    /// </summary>
    public static void Set(uint entityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Target, entityID);

    /// <summary>
    ///     清除当前目标
    /// </summary>
    public static void Clear() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Target, 0xE0000000);
}
