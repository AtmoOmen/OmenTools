using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ReviveCommand : ExecuteCommandBase
{
    /// <summary>
    ///     接受复活
    /// </summary>
    public static void Accept() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Revive, 5);

    /// <summary>
    ///     确认返回返回点
    /// </summary>
    public static void ReturnToHomePoint() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.Revive, 8);
}
