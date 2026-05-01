using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ResetStrikingDummyCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.ResetStrikingDummy;

    /// <summary>
    ///     清除来自木人的仇恨
    /// </summary>
    public void Reset(uint strikingDummyobjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, strikingDummyobjectID);
}
