using Lumina.Excel.Sheets;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AbandonLeveQuestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     放弃理符任务
    /// </summary>
    /// <seealso cref="Leve" />
    public static void Abandon(uint leveID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AbandonLeveQuest, leveID);
}
