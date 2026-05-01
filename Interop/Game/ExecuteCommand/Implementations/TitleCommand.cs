using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TitleCommand : ExecuteCommandBase
{
    /// <summary>
    ///     更改佩戴的称号
    /// </summary>
    /// <seealso cref="Title" />
    public static void Change(uint titleID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ChangeTitle, titleID);
}
