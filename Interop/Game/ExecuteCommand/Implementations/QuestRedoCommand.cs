using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class QuestRedoCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入昔日重现章节
    /// </summary>
    public static void Start(uint questRedoRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.QuestRedo, questRedoRowID);

    /// <summary>
    ///     退出昔日重现
    /// </summary>
    public static void Exit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.QuestRedo);
}
