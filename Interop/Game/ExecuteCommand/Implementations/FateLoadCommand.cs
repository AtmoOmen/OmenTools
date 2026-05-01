using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateLoadCommand : ExecuteCommandBase
{
    /// <summary>
    ///     加载临危受命信息
    /// </summary>
    public static void Load(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateLoad, fateID);
}
