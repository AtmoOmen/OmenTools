using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ReviveCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.Revive;

    /// <summary>
    ///     接受复活
    /// </summary>
    public void Accept() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 5);

    /// <summary>
    ///     确认返回返回点
    /// </summary>
    public void ReturnToHomePoint() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 8);
}