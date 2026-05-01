using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestContentsNoteCategoryCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestContentsNoteCategory;

    /// <summary>
    ///     请求挑战笔记具体类别下数据
    /// </summary>
    public void Request(uint categoryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, categoryIndex);
}