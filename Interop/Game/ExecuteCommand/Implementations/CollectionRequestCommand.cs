using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CollectionRequestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求部分物品的解锁状态
    /// </summary>
    public static void RequestItemActionUnlockState() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestItemActionUnlockState);

    /// <summary>
    ///     请求肖像列表数据
    /// </summary>
    public static void RequestPortraits() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestPortrait);

    /// <summary>
    ///     请求铭牌数据
    /// </summary>
    public static void RequestCharaCard() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestCharacterCard);
}
