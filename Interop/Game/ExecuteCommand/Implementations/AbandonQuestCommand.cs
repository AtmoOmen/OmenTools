using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AbandonQuestCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.AbandonQuest;

    /// <summary>
    ///     放弃任务
    /// </summary>
    /// <seealso cref="Quest" />
    public void Abandon(uint questID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, questID);
}
