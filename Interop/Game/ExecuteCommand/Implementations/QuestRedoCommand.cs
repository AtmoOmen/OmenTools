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

    /// <summary>
    ///     继续先前的昔日重现
    /// </summary>
    public static void Continue() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ContinueQuestRedo);

    /// <summary>
    ///     删除已有的昔日重现存档
    /// </summary>
    public static void DeleteSave() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DeleteQuestRedoSave);

    /// <summary>
    ///     初始化昔日重现所需的界面信息
    /// </summary>
    public static void ResetUI() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ResetQuestRedoUI);
}
