using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BuybackReconstrcutionItemCommand : ExecuteCommandBase
{
    /// <summary>
    ///     买回多玛飞地支援物资
    /// </summary>
    public static void Buyback(uint itemIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.BuybackReconstrcutionItem, itemIndex);
}
