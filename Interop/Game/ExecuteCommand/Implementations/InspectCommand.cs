using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class InspectCommand : ExecuteCommandBase
{
    /// <summary>
    ///     检查指定玩家
    /// </summary>
    public static void Inspect(uint objectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Inspect, objectID);
}
