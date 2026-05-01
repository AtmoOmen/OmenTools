using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateStartCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.FateStart;

    /// <summary>
    ///     开始指定的临危受命任务
    /// </summary>
    public void Start(uint fateID, uint targetObjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, fateID, targetObjectID);
}
