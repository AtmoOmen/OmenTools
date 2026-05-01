using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateEnterCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.FateEnter;

    /// <summary>
    ///     进入临危受命范围
    /// </summary>
    public void Enter(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, fateID);
}
