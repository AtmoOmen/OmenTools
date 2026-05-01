using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class InspectCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Inspect;

    /// <summary>
    ///     检查指定玩家
    /// </summary>
    public void Inspect(uint objectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, objectID);
}
