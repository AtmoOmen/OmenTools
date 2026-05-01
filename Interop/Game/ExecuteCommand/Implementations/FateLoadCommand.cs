using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateLoadCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.FateLoad;

    /// <summary>
    ///     加载临危受命信息
    /// </summary>
    public void Load(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, fateID);
}