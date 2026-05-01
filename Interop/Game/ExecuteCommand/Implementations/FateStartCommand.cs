using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateStartCommand : ExecuteCommandBase
{
    /// <summary>
    ///     开始指定的临危受命任务
    /// </summary>
    public static void Start(uint fateID, uint targetObjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateStart, fateID, targetObjectID);
}
