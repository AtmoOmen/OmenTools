using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ChangeTitleCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.ChangeTitle;

    /// <summary>
    ///     更改佩戴的称号
    /// </summary>
    /// <seealso cref="Title" />
    public void Change(uint titleID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, titleID);
}
