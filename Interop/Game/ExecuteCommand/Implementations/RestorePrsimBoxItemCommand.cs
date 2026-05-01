using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RestorePrsimBoxItemCommand : ExecuteCommandBase
{
    /// <summary>
    ///     取出投影台物品
    /// </summary>
    public static void Restore(uint prismBoxItemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RestorePrsimBoxItem, prismBoxItemID);
}
