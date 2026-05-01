using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class QuestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     放弃任务
    /// </summary>
    /// <seealso cref="Quest" />
    public static void Abandon(uint questID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AbandonQuest, questID);
}
