using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class QuestRedoCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.QuestRedo;

    /// <summary>
    ///     进入昔日重现章节
    /// </summary>
    public void Start(uint questRedoRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, questRedoRowID);

    /// <summary>
    ///     退出昔日重现
    /// </summary>
    public void Exit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag);
}
