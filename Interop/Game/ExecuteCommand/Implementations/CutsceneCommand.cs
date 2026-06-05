using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CutsceneCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求过场剧情数据
    /// </summary>
    public static void Request(uint cutsceneID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestCutsceneInfo307, cutsceneID);

    /// <summary>
    ///     请求过场剧情数据
    /// </summary>
    public static void RequestByIndex(uint cutsceneIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestCutscene831, cutsceneIndex);
}
