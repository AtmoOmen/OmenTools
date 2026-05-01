using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BuybackReconstrcutionItemCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.BuybackReconstrcutionItem;

    /// <summary>
    ///     买回支援物资
    /// </summary>
    public void Buyback(uint itemIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, itemIndex);
}