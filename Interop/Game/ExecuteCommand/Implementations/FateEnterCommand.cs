using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateEnterCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入临危受命范围
    /// </summary>
    public static void Enter(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateEnter, fateID);
}
