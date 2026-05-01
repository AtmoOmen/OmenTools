using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SetRetainerMarketPriceCommand : ExecuteCommandBase
{
    /// <summary>
    ///     设置当前雇员市场出售物品价格
    /// </summary>
    public static void Set(uint slot, uint price) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetRetainerMarketPrice, slot, price);
}
