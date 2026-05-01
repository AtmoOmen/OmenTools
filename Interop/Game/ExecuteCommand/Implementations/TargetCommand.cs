using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TargetCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Target;

    /// <summary>
    ///     选中目标
    /// </summary>
    public void Set(uint entityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, entityID);

    /// <summary>
    ///     清除当前目标
    /// </summary>
    public void Clear() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0xE0000000);
}