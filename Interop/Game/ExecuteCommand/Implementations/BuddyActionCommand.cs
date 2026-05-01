using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BuddyActionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.BuddyAction;

    /// <summary>
    ///     切换陆行鸟作战风格
    /// </summary>
    public void Execute(uint buddyActionRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, buddyActionRowID);
}