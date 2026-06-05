using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BlueMageRequestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求青魔法师每周挑战信息
    /// </summary>
    public static void RequestContentBriefing() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestBlueContentBriefing);

    /// <summary>
    ///     请求青魔法书数据
    /// </summary>
    public static void RequestNotebook() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequstBlueNotebook);
}
